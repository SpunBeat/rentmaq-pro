using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipment");

        builder.HasKey(x => x.EquipmentId);

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.AssetTag)
            .HasColumnName("asset_tag")
            .HasMaxLength(50);

        builder.Property(x => x.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(100);

        builder.Property(x => x.Make)
            .HasColumnName("make")
            .HasMaxLength(50);

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasMaxLength(50);

        builder.Property(x => x.Year)
            .HasColumnName("year");

        builder.Property(x => x.EquipmentType)
            .HasColumnName("equipment_type");

        builder.Property(x => x.WeightTons)
            .HasColumnName("weight_tons")
            .HasPrecision(6, 2);

        builder.Property(x => x.AcquisitionCost)
            .HasColumnName("acquisition_cost")
            .HasPrecision(15, 2);

        builder.Property(x => x.AcquisitionDate)
            .HasColumnName("acquisition_date");

        builder.Property(x => x.CurrentStatus)
            .HasColumnName("current_status");

        builder.Property(x => x.AempEndpointUrl)
            .HasColumnName("aemp_endpoint_url")
            .HasMaxLength(500);

        builder.Property(x => x.LoadProfileId)
            .HasColumnName("load_profile_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.AssetTag).IsUnique();
        builder.HasIndex(x => x.SerialNumber).IsUnique();
    }
}
