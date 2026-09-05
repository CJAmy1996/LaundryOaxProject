using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace LaundryOaxWebAPI.IoT;
public static class FleetEndpoints
{
    public static void MapFleet(this WebApplication app)
    {
        var api = app.MapGroup("/api/fleet").RequireAuthorization("FleetRead").RequireRateLimiting("api");
        api.MapGet("/snapshot", async (ClaimsPrincipal user, IDbContextFactory<FleetDb> factory, CancellationToken ct) =>
        {
            var tenant = user.FindFirstValue("tenant_id")!;
            await using var db = await factory.CreateDbContextAsync(ct);
            return Results.Ok(new {
                tenant, demo = app.Configuration.GetValue<bool>("Demo:Enabled"), serverTime = DateTime.UtcNow,
                machines = await db.Machines.AsNoTracking().Where(x => x.TenantId == tenant).OrderBy(x => x.DeviceId).ToListAsync(ct),
                received = await db.Events.CountAsync(x => x.TenantId == tenant, ct),
                pending = await db.Events.CountAsync(x => x.TenantId == tenant && x.ProcessedUtc == null, ct),
                notifications = await db.Notifications.AsNoTracking().Where(x => x.TenantId == tenant).OrderByDescending(x => x.CreatedUtc).Take(30).ToListAsync(ct),
                events = await db.Events.AsNoTracking().Where(x => x.TenantId == tenant).OrderByDescending(x => x.ReceivedUtc)
                    .Select(x => new { x.EventId, x.DeviceId, x.Sequence, x.ReceivedUtc, x.ProcessedUtc }).Take(30).ToListAsync(ct)
            });
        });
        api.MapGet("/audit", async (ClaimsPrincipal user, IDbContextFactory<FleetDb> factory, CancellationToken ct) => {
            await using var db = await factory.CreateDbContextAsync(ct);
            var tenant = user.FindFirstValue("tenant_id");
            return await db.Audit.AsNoTracking().Where(x => x.TenantId == tenant).OrderByDescending(x => x.Id).Take(100).ToListAsync(ct);
        }).RequireAuthorization("Operator");
        api.MapPost("/notifications/{id:guid}/read", async (Guid id, ClaimsPrincipal user, IDbContextFactory<FleetDb> factory, CancellationToken ct) => {
            await using var db = await factory.CreateDbContextAsync(ct);
            var notification = await db.Notifications.FindAsync([user.FindFirstValue("tenant_id")!, id], ct);
            if (notification == null) return Results.NotFound();
            notification.Read = true;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        api.MapPost("/machines", async (MachineRegistration input, ClaimsPrincipal user, IDbContextFactory<FleetDb> factory, CancellationToken ct) => {
            if (string.IsNullOrWhiteSpace(input.DeviceId) || input.DeviceId.Length > 64 ||
                input.Kind is not ("washer" or "dryer") || string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 100 ||
                string.IsNullOrWhiteSpace(input.Location) || input.Location.Length > 100) return Results.BadRequest(new { error = "Invalid machine details." });
            var tenant = user.FindFirstValue("tenant_id")!;
            await using var db = await factory.CreateDbContextAsync(ct);
            if (await db.Machines.FindAsync([tenant, input.DeviceId], ct) != null) return Results.Conflict();
            db.Machines.Add(new() { TenantId = tenant, DeviceId = input.DeviceId, Name = input.Name, Kind = input.Kind, Location = input.Location });
            db.Audit.Add(new() { TenantId = tenant, Action = "machine.registered", Subject = input.DeviceId });
            await db.SaveChangesAsync(ct);
            return Results.Created("/api/fleet/snapshot", new { input.DeviceId });
        }).RequireAuthorization("Operator");
        app.MapPost("/api/telemetry", async (Telemetry e, ClaimsPrincipal user, EventPipeline pipeline, CancellationToken ct) => {
            if (user.FindFirstValue("tenant_id") != e.TenantId || user.FindFirstValue("device_id") != e.DeviceId)
                return Results.Forbid();
            try { return Results.Ok(await pipeline.Accept(e, ct)); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
            catch (KeyNotFoundException) { return Results.NotFound(); }
            catch (EventConflictException ex) { return Results.Conflict(new { error = ex.Message }); }
        }).RequireAuthorization("Device").RequireRateLimiting("api");

        // Polling-backed SSE works across API replicas because the source of truth is the database.
        api.MapGet("/stream", async (HttpContext context, IDbContextFactory<FleetDb> factory) => {
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers["X-Accel-Buffering"] = "no";
            var tenant = context.User.FindFirstValue("tenant_id");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
            timeout.CancelAfter(TimeSpan.FromMinutes(2)); // reconnect revalidates token/session
            try {
                while (!timeout.IsCancellationRequested) {
                    await using var db = await factory.CreateDbContextAsync(timeout.Token);
                    var count = await db.Events.CountAsync(x => x.TenantId == tenant && x.ProcessedUtc != null, timeout.Token);
                    await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(new { processed = count })}\n\n", timeout.Token);
                    await context.Response.Body.FlushAsync(timeout.Token);
                    await Task.Delay(2000, timeout.Token);
                }
            } catch (OperationCanceledException) { }
        });
    }
}
public sealed record MachineRegistration(string DeviceId, string Name, string Kind, string Location);
