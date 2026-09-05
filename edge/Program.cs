using LaundryOax.Edge;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
var outbox = new Outbox(Environment.GetEnvironmentVariable("EDGE_DB") ?? "edge.db");
var tenant = Environment.GetEnvironmentVariable("EDGE_TENANT") ?? "oaxaca-central";
var device = Environment.GetEnvironmentVariable("EDGE_DEVICE") ?? "W-01";
if (args.Contains("capture")) {
    // Replace these readings with the appliance's sensor driver; capture works with no network.
    var e = outbox.Capture(tenant, device, "washing", 25, 32, 65, 100);
    Console.WriteLine($"Durably captured {e.EventId}, sequence {e.Sequence}. Pending: {outbox.Count()}");
    return;
}
if (!args.Contains("replay")) { Console.WriteLine("Usage: dotnet run -- capture | replay. Configure EDGE_DB, EDGE_TENANT, EDGE_DEVICE, EDGE_API, EDGE_KEY."); return; }
var endpoint = new Uri(Environment.GetEnvironmentVariable("EDGE_API") ?? "https://localhost:7106/api/telemetry");
if (endpoint.Scheme != "https" && !endpoint.IsLoopback) throw new InvalidOperationException("Remote ingestion requires HTTPS.");
var key = Environment.GetEnvironmentVariable("EDGE_KEY") ?? throw new InvalidOperationException("EDGE_KEY is required.");
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
client.DefaultRequestHeaders.Add("X-Device-Key", key);
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.Cancel(); };
Console.WriteLine("Replaying durable events. Ctrl+C stops safely.");
try {
    while (!shutdown.IsCancellationRequested) {
        var e = outbox.Next();
        if (e == null) { await Task.Delay(1000, shutdown.Token); continue; }
        try {
            using var response = await client.PostAsync(endpoint, new StringContent(e.Payload, Encoding.UTF8, "application/json"), shutdown.Token);
            if (response.IsSuccessStatusCode) {
                var receipt = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: shutdown.Token);
                if (receipt.GetProperty("eventId").GetString() != e.EventId) throw new InvalidOperationException("ACK event ID mismatch");
                outbox.Acknowledge(e.EventId);
                Console.WriteLine($"Acknowledged {e.EventId}; pending {outbox.Count()}");
            } else {
                outbox.Retry(e);
                Console.Error.WriteLine($"Ingestion returned {(int)response.StatusCode}; event retained. 4xx errors require provisioning/payload repair.");
            }
        } catch (Exception ex) when (ex is not OperationCanceledException || !shutdown.IsCancellationRequested) {
            outbox.Retry(e); Console.Error.WriteLine($"Delivery failed ({ex.GetType().Name}); retained for retry.");
        }
    }
} catch (OperationCanceledException) when (shutdown.IsCancellationRequested) { }
