using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class TelemetryAlert
{
    public Guid AlertId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid? SnapshotId { get; set; }
    public DateTimeOffset DetectedAt { get; set; }

    public AlertTypeEnum AlertType { get; set; }
    public SeverityEnum Severity { get; set; }

    // J1939 (only for FaultCode)
    public int? Spn { get; set; }
    public int? Fmi { get; set; }

    // Threshold values
    public decimal? ThresholdValue { get; set; }
    public decimal? ActualValue { get; set; }

    // Impact-specific
    public decimal? ImpactGX { get; set; }
    public decimal? ImpactGY { get; set; }
    public decimal? ImpactGZ { get; set; }

    // Tilt-specific
    public decimal? TiltLateralDeg { get; set; }
    public decimal? TiltLongitudinalDeg { get; set; }

    public bool AttributedToTenant { get; set; }
    public string? Description { get; set; }
    public bool Acknowledged { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public bool Resolved { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
}
