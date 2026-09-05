using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace LaundryOaxWebAPI.IoT;

public sealed record Receipt(Guid EventId, long Sequence, bool Duplicate);
public sealed class EventConflictException(string message) : Exception(message);
public sealed class EventPipeline(IDbContextFactory<FleetDb> factory, ILogger<EventPipeline> logger)
{
    // Serializes SQLite writers within the local process. SQL Server also enforces unique keys
    // and serializable transactions across replicas; callers retry transient conflicts.
    private readonly SemaphoreSlim gate = new(1, 1);
    public async Task<Receipt> Accept(Telemetry e, CancellationToken ct = default)
    {
        if (TelemetryRules.Validate(e) is { } error) throw new ArgumentException(error);
        var payload = JsonSerializer.Serialize(e);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        await gate.WaitAsync(ct);
        try
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var existing = await db.Events.FindAsync([e.TenantId, e.EventId], ct);
            if (existing != null)
            {
                if (existing.Hash != hash) throw new EventConflictException("Event ID reused with different content.");
                return new(e.EventId, e.Sequence, true);
            }
            var machine = await db.Machines.FindAsync([e.TenantId, e.DeviceId], ct)
                ?? throw new KeyNotFoundException("Device is not registered for this tenant.");
            if ((machine.Kind == "washer" && e.Phase == "drying") ||
                (machine.Kind == "dryer" && e.Phase is "washing" or "rinsing" or "spinning"))
                throw new ArgumentException("Cycle phase does not match appliance type.");
            if (e.Sequence <= machine.LastSequence || await db.Events.AnyAsync(x =>
                x.TenantId == e.TenantId && x.DeviceId == e.DeviceId && x.Sequence == e.Sequence, ct))
                throw new EventConflictException("Sequence already belongs to a different event.");
            db.Events.Add(new() { EventId = e.EventId, TenantId = e.TenantId, DeviceId = e.DeviceId,
                Sequence = e.Sequence, Payload = payload, Hash = hash, ReceivedUtc = DateTime.UtcNow });
            db.Audit.Add(new() { TenantId = e.TenantId, Action = "telemetry.accepted", Subject = e.DeviceId + "/" + e.Sequence });
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException)
            {
                // A competing API replica may have committed the same event first.
                await using var check = await factory.CreateDbContextAsync(ct);
                var winner = await check.Events.FindAsync([e.TenantId, e.EventId], ct);
                if (winner?.Hash == hash) return new(e.EventId, e.Sequence, true);
                throw;
            }
            logger.LogInformation("Telemetry persisted {Tenant} {Device} {Sequence}", e.TenantId, e.DeviceId, e.Sequence);
            return new(e.EventId, e.Sequence, false); // ACK only after durable commit.
        }
        finally { gate.Release(); }
    }

    public async Task<int> Process(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct);
        try
        {
            var count = 0;
            // A batch is bounded so requests cannot be starved by a large replay.
            for (var i = 0; i < 100; i++)
            {
                await using var db = await factory.CreateDbContextAsync(ct);
                await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                var next = await (from ev in db.Events
                    join m in db.Machines on new { ev.TenantId, ev.DeviceId } equals new { m.TenantId, m.DeviceId }
                    where ev.ProcessedUtc == null && ev.Sequence == m.LastSequence + 1
                    orderby ev.ReceivedUtc
                    select ev).FirstOrDefaultAsync(ct);
                if (next == null) break;
                var e = JsonSerializer.Deserialize<Telemetry>(next.Payload)!;
                var machine = await db.Machines.FindAsync([e.TenantId, e.DeviceId], ct);
                var previous = machine!.Phase;
                machine.Phase = e.Phase; machine.Progress = e.Progress;
                machine.TemperatureC = e.TemperatureC; machine.MoisturePercent = e.MoisturePercent;
                machine.TurbidityNtu = e.TurbidityNtu; machine.LastSequence = e.Sequence;
                machine.LastSeenUtc = e.ObservedAt.UtcDateTime;
                machine.EstimatedMinutes = TelemetryRules.EstimateMinutes(e);
                machine.ExtraRinses = TelemetryRules.EstimateRinses(e);
                if (e.Phase != previous && e.Phase is "complete" or "fault")
                    db.Notifications.Add(new() { Id = e.EventId, TenantId = e.TenantId, DeviceId = e.DeviceId,
                        Message = e.Phase == "complete" ? $"{machine.Name}: cycle complete. Ready for pickup." : $"{machine.Name}: appliance needs attention.",
                        CreatedUtc = DateTime.UtcNow });
                next.ProcessedUtc = DateTime.UtcNow;
                db.Audit.Add(new() { TenantId = e.TenantId, Action = "telemetry.processed", Subject = e.DeviceId + "/" + e.Sequence });
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                count++;
            }
            return count;
        }
        finally { gate.Release(); }
    }
}

public sealed class IngestionWorker(EventPipeline pipeline, ILogger<IngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await pipeline.Process(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Processing failed; committed events remain available for retry"); }
            await Task.Delay(500, stoppingToken);
        }
    }
}
