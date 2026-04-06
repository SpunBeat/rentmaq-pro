using NetTopologySuite.Geometries;

namespace RentMaq.Domain.Entities;

public class TelemetryReading
{
    public Guid SnapshotId { get; set; }
    public Guid EquipmentId { get; set; }
    public DateTimeOffset RecordedAt { get; set; }

    // Location (ISO 15143-3)
    public Point Location { get; set; } = null!;
    public decimal? Altitude { get; set; }
    public decimal? Heading { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? Hdop { get; set; }
    public int? Satellites { get; set; }

    // Horometers (ISO 15143-3)
    public decimal? EngineHours { get; set; }
    public decimal? PtoHours { get; set; }
    public decimal? IdleHours { get; set; }
    public decimal? CumulativeIdleHours { get; set; }
    public decimal? CumulativeIdleNonOperatingHours { get; set; }

    // Consumption & Distance
    public decimal? Distance { get; set; }
    public decimal? FuelUsed { get; set; }
    public decimal? FuelLevel { get; set; }
    public decimal? DefUsed { get; set; }
    public decimal? DefLevel { get; set; }
    public int? EngineStatus { get; set; }
    public decimal? LoadFactor { get; set; }
    public string? ActiveSwitches { get; set; } // JSONB

    // Complementary Sensors
    public decimal? ImpactGX { get; set; }
    public decimal? ImpactGY { get; set; }
    public decimal? ImpactGZ { get; set; }
    public decimal? TiltLateral { get; set; }
    public decimal? TiltLongitudinal { get; set; }
    public decimal? HydraulicPressure { get; set; }
    public decimal? EngineTemperature { get; set; }
    public decimal? AmbientTemperature { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
}
