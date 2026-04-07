using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class MaintenanceScheduleConfiguration : IEntityTypeConfiguration<MaintenanceSchedule>
{
    public void Configure(EntityTypeBuilder<MaintenanceSchedule> builder)
    {
        builder.ToTable("maintenance_schedules");

        builder.HasKey(x => x.ScheduleId);

        builder.Property(x => x.ScheduleId)
            .HasColumnName("schedule_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.ServiceTier)
            .HasColumnName("service_tier")
            .HasMaxLength(50);

        builder.Property(x => x.IntervalHours)
            .HasColumnName("interval_hours");

        builder.Property(x => x.IntervalDays)
            .HasColumnName("interval_days");

        builder.Property(x => x.LastServiceHours)
            .HasColumnName("last_service_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.LastServiceDate)
            .HasColumnName("last_service_date");

        builder.Property(x => x.NextDueHours)
            .HasColumnName("next_due_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.NextDueDate)
            .HasColumnName("next_due_date");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
