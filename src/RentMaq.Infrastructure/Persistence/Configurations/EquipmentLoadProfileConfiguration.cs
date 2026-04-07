using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class EquipmentLoadProfileConfiguration : IEntityTypeConfiguration<EquipmentLoadProfile>
{
    public void Configure(EntityTypeBuilder<EquipmentLoadProfile> builder)
    {
        builder.ToTable("equipment_load_profiles");

        builder.HasKey(x => x.ProfileId);

        builder.Property(x => x.ProfileId)
            .HasColumnName("profile_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentMake)
            .HasColumnName("equipment_make")
            .HasMaxLength(50);

        builder.Property(x => x.EquipmentModel)
            .HasColumnName("equipment_model")
            .HasMaxLength(50);

        builder.Property(x => x.ApplicationType)
            .HasColumnName("application_type")
            .HasMaxLength(50);

        builder.Property(x => x.EngineLoadFactor)
            .HasColumnName("engine_load_factor")
            .HasPrecision(5, 3);

        builder.Property(x => x.PtoLoadFactor)
            .HasColumnName("pto_load_factor")
            .HasPrecision(5, 3);

        builder.Property(x => x.MaintenanceIntervalHours)
            .HasColumnName("maintenance_interval_hours");

        builder.Property(x => x.TargetUtilizationPct)
            .HasColumnName("target_utilization_pct")
            .HasPrecision(5, 2);

        builder.Property(x => x.LastUpdated)
            .HasColumnName("last_updated")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => new { x.EquipmentMake, x.EquipmentModel, x.ApplicationType }).IsUnique();
    }
}
