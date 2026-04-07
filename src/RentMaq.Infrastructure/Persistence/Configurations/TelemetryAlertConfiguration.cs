using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class TelemetryAlertConfiguration : IEntityTypeConfiguration<TelemetryAlert>
{
    public void Configure(EntityTypeBuilder<TelemetryAlert> builder)
    {
        builder.ToTable("telemetry_alerts");

        builder.HasKey(x => x.AlertId);

        builder.Property(x => x.AlertId)
            .HasColumnName("alert_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.SnapshotId)
            .HasColumnName("snapshot_id");

        builder.Property(x => x.DetectedAt)
            .HasColumnName("detected_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.AlertType)
            .HasColumnName("alert_type");

        builder.Property(x => x.Severity)
            .HasColumnName("severity");

        builder.Property(x => x.Spn)
            .HasColumnName("spn");

        builder.Property(x => x.Fmi)
            .HasColumnName("fmi");

        builder.Property(x => x.ThresholdValue)
            .HasColumnName("threshold_value")
            .HasPrecision(10, 3);

        builder.Property(x => x.ActualValue)
            .HasColumnName("actual_value")
            .HasPrecision(10, 3);

        builder.Property(x => x.ImpactGX)
            .HasColumnName("impact_g_x")
            .HasPrecision(5, 2);

        builder.Property(x => x.ImpactGY)
            .HasColumnName("impact_g_y")
            .HasPrecision(5, 2);

        builder.Property(x => x.ImpactGZ)
            .HasColumnName("impact_g_z")
            .HasPrecision(5, 2);

        builder.Property(x => x.TiltLateralDeg)
            .HasColumnName("tilt_lateral_deg")
            .HasPrecision(5, 2);

        builder.Property(x => x.TiltLongitudinalDeg)
            .HasColumnName("tilt_longitudinal_deg")
            .HasPrecision(5, 2);

        builder.Property(x => x.AttributedToTenant)
            .HasColumnName("attributed_to_tenant")
            .HasDefaultValue(false);

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.Acknowledged)
            .HasColumnName("acknowledged")
            .HasDefaultValue(false);

        builder.Property(x => x.AcknowledgedBy)
            .HasColumnName("acknowledged_by");

        builder.Property(x => x.AcknowledgedAt)
            .HasColumnName("acknowledged_at");

        builder.Property(x => x.Resolved)
            .HasColumnName("resolved")
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.EquipmentId, x.Severity, x.Resolved });
        builder.HasIndex(x => x.DetectedAt).IsDescending();
    }
}
