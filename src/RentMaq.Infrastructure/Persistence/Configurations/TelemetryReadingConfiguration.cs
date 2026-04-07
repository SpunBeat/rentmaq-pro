using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class TelemetryReadingConfiguration : IEntityTypeConfiguration<TelemetryReading>
{
    public void Configure(EntityTypeBuilder<TelemetryReading> builder)
    {
        builder.ToTable("telemetry_readings");

        builder.HasKey(x => new { x.SnapshotId, x.RecordedAt });

        builder.Property(x => x.SnapshotId)
            .HasColumnName("snapshot_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.RecordedAt)
            .HasColumnName("recorded_at");

        builder.Property(x => x.Location)
            .HasColumnName("location")
            .HasColumnType("geography(point, 4326)");

        builder.Property(x => x.Altitude)
            .HasColumnName("altitude")
            .HasPrecision(8, 2);

        builder.Property(x => x.Heading)
            .HasColumnName("heading")
            .HasPrecision(5, 2);

        builder.Property(x => x.SpeedKmh)
            .HasColumnName("speed_kmh")
            .HasPrecision(5, 2);

        builder.Property(x => x.Hdop)
            .HasColumnName("hdop")
            .HasPrecision(4, 2);

        builder.Property(x => x.Satellites)
            .HasColumnName("satellites");

        builder.Property(x => x.EngineHours)
            .HasColumnName("engine_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.PtoHours)
            .HasColumnName("pto_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.IdleHours)
            .HasColumnName("idle_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.CumulativeIdleHours)
            .HasColumnName("cumulative_idle_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.CumulativeIdleNonOperatingHours)
            .HasColumnName("cumulative_idle_non_operating_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.Distance)
            .HasColumnName("distance")
            .HasPrecision(10, 2);

        builder.Property(x => x.FuelUsed)
            .HasColumnName("fuel_used")
            .HasPrecision(10, 2);

        builder.Property(x => x.FuelLevel)
            .HasColumnName("fuel_level")
            .HasPrecision(5, 2);

        builder.Property(x => x.DefUsed)
            .HasColumnName("def_used")
            .HasPrecision(10, 2);

        builder.Property(x => x.DefLevel)
            .HasColumnName("def_level")
            .HasPrecision(5, 2);

        builder.Property(x => x.EngineStatus)
            .HasColumnName("engine_status");

        builder.Property(x => x.LoadFactor)
            .HasColumnName("load_factor")
            .HasPrecision(5, 2);

        builder.Property(x => x.ActiveSwitches)
            .HasColumnName("active_switches")
            .HasColumnType("jsonb");

        builder.Property(x => x.ImpactGX)
            .HasColumnName("impact_g_x")
            .HasPrecision(5, 2);

        builder.Property(x => x.ImpactGY)
            .HasColumnName("impact_g_y")
            .HasPrecision(5, 2);

        builder.Property(x => x.ImpactGZ)
            .HasColumnName("impact_g_z")
            .HasPrecision(5, 2);

        builder.Property(x => x.TiltLateral)
            .HasColumnName("tilt_lateral")
            .HasPrecision(5, 2);

        builder.Property(x => x.TiltLongitudinal)
            .HasColumnName("tilt_longitudinal")
            .HasPrecision(5, 2);

        builder.Property(x => x.HydraulicPressure)
            .HasColumnName("hydraulic_pressure")
            .HasPrecision(10, 2);

        builder.Property(x => x.EngineTemperature)
            .HasColumnName("engine_temperature")
            .HasPrecision(5, 2);

        builder.Property(x => x.AmbientTemperature)
            .HasColumnName("ambient_temperature")
            .HasPrecision(5, 2);
    }
}
