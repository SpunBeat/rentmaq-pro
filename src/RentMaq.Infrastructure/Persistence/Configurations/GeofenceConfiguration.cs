using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class GeofenceConfiguration : IEntityTypeConfiguration<Geofence>
{
    public void Configure(EntityTypeBuilder<Geofence> builder)
    {
        builder.ToTable("geofences");

        builder.HasKey(x => x.GeofenceId);

        builder.Property(x => x.GeofenceId)
            .HasColumnName("geofence_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100);

        builder.Property(x => x.Perimeter)
            .HasColumnName("perimeter")
            .HasColumnType("geography(polygon, 4326)");

        builder.Property(x => x.IsYard)
            .HasColumnName("is_yard")
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
