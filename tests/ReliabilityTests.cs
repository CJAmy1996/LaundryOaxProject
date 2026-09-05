using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using LaundryOax.Edge;
using LaundryOaxWebAPI.IoT;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

public sealed class FleetFixture : WebApplicationFactory<Program>
{
    public readonly string DbPath = Path.Combine(Path.GetTempPath(), $"laundry-tests-{Guid.NewGuid()}.db");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string,string?> {
            ["Demo:Enabled"] = "true", ["ConnectionStrings:Fleet"] = $"Data Source={DbPath}",
            ["Devices:0:TenantId"] = "oaxaca-central", ["Devices:0:DeviceId"] = "W-01",
            ["Devices:0:KeySha256"] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("test-device-key")))
        }));
        builder.ConfigureServices(s => {
            s.AddAuthentication(o => { o.DefaultAuthenticateScheme = "TestUser"; o.DefaultChallengeScheme = "TestUser"; })
                .AddScheme<AuthenticationSchemeOptions, TestUser>("TestUser", _ => { });
            // Deterministic processing: tests explicitly invoke a fresh worker pass.
            foreach (var descriptor in s.Where(x => x.ServiceType == typeof(IHostedService) &&
                (x.ImplementationType == typeof(IngestionWorker) || x.ImplementationType == typeof(SqsWorker))).ToArray()) s.Remove(descriptor);
        });
    }
    public EventPipeline Pipeline => Services.GetRequiredService<EventPipeline>();
    public async Task<FleetDb> Db() => await Services.GetRequiredService<IDbContextFactory<FleetDb>>().CreateDbContextAsync();
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var file in new[] { DbPath, DbPath + "-shm", DbPath + "-wal" }) if (File.Exists(file)) File.Delete(file);
    }
}
public sealed class TestUser(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-No-User")) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new[] { new Claim("tenant_id", Request.Headers["X-Test-Tenant"].FirstOrDefault() ?? "oaxaca-central"),
            new Claim(ClaimTypes.Role, Request.Headers["X-Test-Role"].FirstOrDefault() ?? "Operator") };
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
    }
}
public class ReliabilityTests
{
    static Telemetry Event(long sequence = 1, string phase = "washing") => new(Guid.NewGuid(), "oaxaca-central", "W-01", sequence,
        DateTimeOffset.UtcNow, phase, phase == "complete" ? 100 : 30, 35, 50, 75);

    [Fact] public async Task Duplicate_event_is_acknowledged_once_and_conflicting_reuse_rejected()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        var e = Event(); Assert.False((await app.Pipeline.Accept(e)).Duplicate);
        Assert.True((await app.Pipeline.Accept(e)).Duplicate);
        await Assert.ThrowsAsync<EventConflictException>(() => app.Pipeline.Accept(e with { Progress = 55 }));
        await Assert.ThrowsAsync<EventConflictException>(() => app.Pipeline.Accept(e with { EventId = Guid.NewGuid() }));
        Assert.Equal(1, await app.Pipeline.Process());
        Assert.True((await app.Pipeline.Accept(e)).Duplicate);
        Assert.Equal(0, await app.Pipeline.Process());
        await using var db = await app.Db(); Assert.Equal(1, await db.Events.CountAsync());
    }
    [Fact] public async Task Out_of_order_events_wait_then_apply_in_sequence()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        await app.Pipeline.Accept(Event(2, "complete")); Assert.Equal(0, await app.Pipeline.Process());
        await app.Pipeline.Accept(Event()); Assert.Equal(2, await app.Pipeline.Process());
        await using var db = await app.Db();
        var m = await db.Machines.FindAsync("oaxaca-central", "W-01");
        Assert.Equal(2, m!.LastSequence); Assert.Equal("complete", m.Phase);
        Assert.Single(await db.Notifications.ToListAsync());
    }
    [Fact] public async Task Worker_restart_preserves_pending_events_and_does_not_repeat_notifications()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        var e = Event(1, "complete"); await app.Pipeline.Accept(e);
        var factory = app.Services.GetRequiredService<IDbContextFactory<FleetDb>>();
        var restarted = new EventPipeline(factory, app.Services.GetRequiredService<ILogger<EventPipeline>>());
        Assert.Equal(1, await restarted.Process());
        await restarted.Accept(e); Assert.Equal(0, await restarted.Process());
        await using var db = await app.Db(); Assert.Single(await db.Notifications.ToListAsync());
    }
    [Fact] public async Task Parallel_duplicate_delivery_only_creates_one_inbox_row()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        var e = Event(); var receipts = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => app.Pipeline.Accept(e)));
        Assert.Single(receipts, x => !x.Duplicate);
        await using var db = await app.Db(); Assert.Equal(1, await db.Events.CountAsync());
    }
    [Fact] public async Task Device_identity_cannot_impersonate_another_device_or_tenant()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/telemetry", Event())).StatusCode);
        client.DefaultRequestHeaders.Add("X-Device-Key", "test-device-key");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/telemetry", Event() with { DeviceId = "W-02" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/telemetry", Event() with { TenantId = "another-tenant" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/telemetry", Event())).StatusCode);
    }
    [Fact] public async Task Tenant_reads_are_isolated_and_viewers_cannot_register_machines()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Tenant", "another-tenant");
        var data = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/fleet/snapshot");
        Assert.Equal(0, data.GetProperty("machines").GetArrayLength());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Viewer");
        var result = await client.PostAsJsonAsync("/api/fleet/machines", new MachineRegistration("W-99", "New washer", "washer", "Centro"));
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    [Fact] public async Task Invalid_sensor_readings_do_not_enter_the_inbox()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        client.DefaultRequestHeaders.Add("X-Device-Key", "test-device-key");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/telemetry", Event() with { Progress = 101 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/telemetry", Event() with { Phase = "drying" })).StatusCode);
        await using var db = await app.Db(); Assert.Equal(0, await db.Events.CountAsync());
    }
    [Fact] public async Task Unknown_notification_cannot_be_acknowledged_cross_tenant()
    {
        using var app = new FleetFixture(); using var client = app.CreateClient();
        var e = Event(1, "complete"); await app.Pipeline.Accept(e); await app.Pipeline.Process();
        client.DefaultRequestHeaders.Add("X-Test-Tenant", "another-tenant");
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/fleet/notifications/{e.EventId}/read", null)).StatusCode);
        await using var db = await app.Db(); Assert.False((await db.Notifications.SingleAsync()).Read);
    }
    [Fact] public void Edge_restart_retains_events_and_allocates_monotonic_sequences()
    {
        var file = Path.Combine(Path.GetTempPath(), $"edge-tests-{Guid.NewGuid()}.db");
        try {
            var edge = new Outbox(file); var first = edge.Capture("tenant", "washer", "washing", 10, 30, 70, 90);
            var restarted = new Outbox(file); var second = restarted.Capture("tenant", "washer", "rinsing", 60, 30, 40, 20);
            Assert.Equal(1, first.Sequence); Assert.Equal(2, second.Sequence); Assert.Equal(2, restarted.Count());
            Assert.Equal(first.EventId.ToString(), restarted.Next()!.EventId);
            restarted.Retry(restarted.Next()!); Assert.Null(restarted.Next()); // Do not skip a backed-off earlier sequence.
            restarted.Acknowledge(first.EventId.ToString());
            Assert.Equal(second.EventId.ToString(), restarted.Next()!.EventId);
            restarted.Acknowledge(second.EventId.ToString()); Assert.Equal(0, new Outbox(file).Count());
            Assert.Equal(3, new Outbox(file).Capture("tenant", "washer", "complete", 100, 30, 5, 5).Sequence);
        } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); foreach(var f in new[]{file,file+"-shm",file+"-wal"}) if(File.Exists(f)) File.Delete(f); }
    }
    [Fact] public void Invalid_edge_capture_rolls_back_sequence_allocation()
    {
        var file = Path.Combine(Path.GetTempPath(), $"edge-tests-{Guid.NewGuid()}.db");
        try {
            var edge = new Outbox(file);
            Assert.Throws<ArgumentException>(() => edge.Capture("t", "d", "washing", 150, 30, 20, 10));
            Assert.Equal(1, edge.Capture("t", "d", "washing", 20, 30, 20, 10).Sequence);
        } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); foreach(var f in new[]{file,file+"-shm",file+"-wal"}) if(File.Exists(f)) File.Delete(f); }
    }
}
