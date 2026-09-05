using Microsoft.Data.Sqlite;
using LaundryOaxWebAPI.IoT;
using System.Text.Json;
namespace LaundryOax.Edge;
public sealed record PendingEvent(string EventId, string Payload, int Attempts);
public sealed class Outbox
{
    private readonly string connectionString;
    public Outbox(string path)
    {
        connectionString = new SqliteConnectionStringBuilder { DataSource = path, DefaultTimeout = 30 }.ToString();
        using var db = Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS sequences (tenant TEXT NOT NULL, device TEXT NOT NULL, value INTEGER NOT NULL, PRIMARY KEY(tenant,device));
            CREATE TABLE IF NOT EXISTS outbox (id TEXT PRIMARY KEY, tenant TEXT NOT NULL, device TEXT NOT NULL,
              sequence INTEGER NOT NULL CHECK(sequence > 0), payload TEXT NOT NULL, attempts INTEGER NOT NULL DEFAULT 0,
              next_attempt INTEGER NOT NULL DEFAULT 0, UNIQUE(tenant,device,sequence));
            """;
        cmd.ExecuteNonQuery();
    }
    private SqliteConnection Open() { var c = new SqliteConnection(connectionString); c.Open(); using var cmd = c.CreateCommand(); cmd.CommandText = "PRAGMA synchronous=FULL;"; cmd.ExecuteNonQuery(); return c; }
    public Telemetry Capture(string tenant, string device, string phase, double progress, double temperature, double moisture, double turbidity)
    {
        using var db = Open();
        using var tx = db.BeginTransaction();
        using var seq = db.CreateCommand(); seq.Transaction = tx;
        seq.CommandText = "INSERT INTO sequences VALUES($t,$d,1) ON CONFLICT(tenant,device) DO UPDATE SET value=value+1 RETURNING value";
        seq.Parameters.AddWithValue("$t", tenant); seq.Parameters.AddWithValue("$d", device);
        var number = (long)seq.ExecuteScalar()!;
        var e = new Telemetry(Guid.NewGuid(), tenant, device, number, DateTimeOffset.UtcNow, phase, progress, temperature, moisture, turbidity);
        if (TelemetryRules.Validate(e) is { } error) throw new ArgumentException(error);
        using var insert = db.CreateCommand(); insert.Transaction = tx;
        insert.CommandText = "INSERT INTO outbox(id,tenant,device,sequence,payload) VALUES($id,$t,$d,$s,$p)";
        insert.Parameters.AddWithValue("$id", e.EventId.ToString()); insert.Parameters.AddWithValue("$t", tenant);
        insert.Parameters.AddWithValue("$d", device); insert.Parameters.AddWithValue("$s", number);
        insert.Parameters.AddWithValue("$p", JsonSerializer.Serialize(e)); insert.ExecuteNonQuery();
        tx.Commit(); // Sequence allocation and event capture succeed or roll back together.
        return e;
    }
    public PendingEvent? Next()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """
          SELECT id,payload,attempts FROM outbox o WHERE next_attempt <= $now
          AND NOT EXISTS (SELECT 1 FROM outbox p WHERE p.tenant=o.tenant AND p.device=o.device AND p.sequence<o.sequence)
          ORDER BY sequence LIMIT 1
          """;
        cmd.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        using var row = cmd.ExecuteReader();
        return row.Read() ? new(row.GetString(0), row.GetString(1), row.GetInt32(2)) : null;
    }
    public void Acknowledge(string id)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM outbox WHERE id=$id"; cmd.Parameters.AddWithValue("$id", id); cmd.ExecuteNonQuery();
    }
    public void Retry(PendingEvent e)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        var delay = Math.Min(300_000, 1000 * Math.Pow(2, Math.Min(e.Attempts, 18))) + Random.Shared.Next(0, 1000);
        cmd.CommandText = "UPDATE outbox SET attempts=attempts+1,next_attempt=$next WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", e.EventId);
        cmd.Parameters.AddWithValue("$next", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)delay); cmd.ExecuteNonQuery();
    }
    public long Count() { using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT COUNT(*) FROM outbox"; return (long)cmd.ExecuteScalar()!; }
}
