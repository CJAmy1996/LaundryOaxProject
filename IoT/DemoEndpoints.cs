using Microsoft.EntityFrameworkCore;
namespace LaundryOaxWebAPI.IoT;
public static class DemoEndpoints
{
    public static async Task Seed(FleetDb db)
    {
        if (await db.Machines.AnyAsync()) return;
        for (var i = 1; i <= 8; i++) db.Machines.Add(new() {
            TenantId = "oaxaca-central", DeviceId = $"{(i <= 4 ? "W" : "D")}-{i:00}",
            Name = (i <= 4 ? "Washer " : "Dryer ") + $"{i:00}", Kind = i <= 4 ? "washer" : "dryer",
            Location = i % 3 == 0 ? "Reforma" : "Centro", Phase = "idle"
        });
        await db.SaveChangesAsync();
    }
    private static readonly SemaphoreSlim Gate = new(1, 1);
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/demo/tick", async (EventPipeline pipeline, IDbContextFactory<FleetDb> factory, CancellationToken ct) => {
            await Gate.WaitAsync(ct);
            try {
                await using var db = await factory.CreateDbContextAsync(ct);
                var machines = await db.Machines.Where(x => x.TenantId == "oaxaca-central").ToListAsync(ct);
                foreach (var m in machines) {
                    var latest = await db.Events.Where(x => x.TenantId == m.TenantId && x.DeviceId == m.DeviceId)
                        .OrderByDescending(x => x.Sequence).FirstOrDefaultAsync(ct);
                    var previous = latest == null ? null : System.Text.Json.JsonSerializer.Deserialize<Telemetry>(latest.Payload);
                    var progress = previous == null || previous.Progress >= 100 ? Random.Shared.Next(8, 25) : Math.Min(100, previous.Progress + 14);
                    var phase = progress >= 100 ? "complete" : m.Kind == "dryer" ? "drying" : progress < 45 ? "washing" : progress < 75 ? "rinsing" : "spinning";
                    await pipeline.Accept(new(Guid.NewGuid(), m.TenantId, m.DeviceId, (latest?.Sequence ?? 0) + 1,
                        DateTimeOffset.UtcNow, phase, progress, m.Kind == "dryer" ? 58 : 32,
                        Math.Max(0, 85 - progress * .8), Math.Max(5, 130 - progress * 1.25)), ct);
                }
                return Results.Ok(new { accepted = machines.Count });
            } finally { Gate.Release(); }
        }).RequireAuthorization("Operator").RequireRateLimiting("api");
    }
}
