using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class EquipmentThresholdConfiguration : IEntityTypeConfiguration<EquipmentThreshold>
{
    public void Configure(EntityTypeBuilder<EquipmentThreshold> builder)
    {
        builder.ToTable("equipment_thresholds");

        builder.HasKey(x => x.ThresholdId);

        builder.Property(x => x.ThresholdId)
            .HasColumnName("threshold_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentType)
            .HasColumnName("equipment_type");

        builder.Property(x => x.EquipmentModel)
            .HasColumnName("equipment_model")
            .HasMaxLength(50);

        builder.Property(x => x.ThresholdType)
            .HasColumnName("threshold_type")
            .HasMaxLength(30);

        builder.Property(x => x.WarningValue)
            .HasColumnName("warning_value")
            .HasPrecision(10, 3);

        builder.Property(x => x.CriticalValue)
            .HasColumnName("critical_value")
            .HasPrecision(10, 3);

        builder.Property(x => x.ShutdownValue)
            .HasColumnName("shutdown_value")
            .HasPrecision(10, 3);

        builder.Property(x => x.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.EquipmentType, x.EquipmentModel, x.ThresholdType }).IsUnique();
    }
}
