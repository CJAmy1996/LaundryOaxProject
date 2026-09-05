using System.ComponentModel.DataAnnotations;
namespace LaundryOaxWebAPI.IoT;

public sealed record Telemetry(Guid EventId, string TenantId, string DeviceId, long Sequence,
    DateTimeOffset ObservedAt, string Phase, double Progress, double TemperatureC,
    double MoisturePercent, double TurbidityNtu);

public sealed class Machine
{
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(64)] public string DeviceId { get; set; } = "";
    [MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(100)] public string Location { get; set; } = "";
    [MaxLength(16)] public string Kind { get; set; } = "washer";
    [MaxLength(24)] public string Phase { get; set; } = "idle";
    public long LastSequence { get; set; }
    public double Progress { get; set; }
    public double TemperatureC { get; set; }
    public double MoisturePercent { get; set; }
    public double TurbidityNtu { get; set; }
    public DateTime LastSeenUtc { get; set; }
    public int EstimatedMinutes { get; set; }
    public int ExtraRinses { get; set; }
}
public sealed class InboxEvent
{
    public Guid EventId { get; set; }
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(64)] public string DeviceId { get; set; } = "";
    public long Sequence { get; set; }
    public string Payload { get; set; } = "";
    [MaxLength(64)] public string Hash { get; set; } = "";
    public DateTime ReceivedUtc { get; set; }
    public DateTime? ProcessedUtc { get; set; }
}
public sealed class CycleNotification
{
    public Guid Id { get; set; }
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(64)] public string DeviceId { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public bool Read { get; set; }
}
public sealed class AuditEntry
{
    public long Id { get; set; }
    [MaxLength(64)] public string TenantId { get; set; } = "";
    public string Action { get; set; } = "";
    public string Subject { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
public static class TelemetryRules
{
    public static readonly string[] Phases = ["idle", "washing", "rinsing", "spinning", "drying", "complete", "fault"];
    public static string? Validate(Telemetry e)
    {
        if (e.EventId == Guid.Empty || string.IsNullOrWhiteSpace(e.TenantId) || e.TenantId.Length > 64 ||
            string.IsNullOrWhiteSpace(e.DeviceId) || e.DeviceId.Length > 64 || e.Sequence < 1)
            return "An event ID, tenant, device and positive sequence are required.";
        if (!Phases.Contains(e.Phase)) return "Unknown cycle phase.";
        if (!double.IsFinite(e.Progress) || e.Progress is < 0 or > 100 ||
            !double.IsFinite(e.TemperatureC) || e.TemperatureC is < -20 or > 150 ||
            !double.IsFinite(e.MoisturePercent) || e.MoisturePercent is < 0 or > 100 ||
            !double.IsFinite(e.TurbidityNtu) || e.TurbidityNtu is < 0 or > 2000)
            return "Sensor readings are outside the supported range.";
        if (e.ObservedAt < DateTimeOffset.UnixEpoch || e.ObservedAt > DateTimeOffset.UtcNow.AddMinutes(5))
            return "Invalid observation timestamp.";
        return null;
    }
    // Transparent baseline heuristics, not calibrated appliance or fabric safety controls.
    public static int EstimateMinutes(Telemetry e) => e.Phase switch {
        "drying" => (int)Math.Ceiling(Math.Max(0, e.MoisturePercent - 5) / 2),
        "washing" or "rinsing" or "spinning" => (int)Math.Ceiling((100 - e.Progress) * .45),
        _ => 0
    };
    public static int EstimateRinses(Telemetry e) => e.Phase is "washing" or "rinsing"
        ? Math.Clamp((int)Math.Ceiling((e.TurbidityNtu - 20) / 50), 0, 3) : 0;
}
