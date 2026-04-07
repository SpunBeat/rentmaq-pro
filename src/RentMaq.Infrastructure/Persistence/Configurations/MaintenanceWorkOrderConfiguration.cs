using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class MaintenanceWorkOrderConfiguration : IEntityTypeConfiguration<MaintenanceWorkOrder>
{
    public void Configure(EntityTypeBuilder<MaintenanceWorkOrder> builder)
    {
        builder.ToTable("maintenance_work_orders", t =>
        {
            t.HasCheckConstraint("chk_trigger_source",
                "trigger_source IN ('TELEMETRY_AUTO','CALENDAR_AUTO','MANUAL_INSPECTION','TENANT_REPORT')");
        });

        builder.HasKey(x => x.WorkOrderId);

        builder.Property(x => x.WorkOrderId)
            .HasColumnName("work_order_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.ScheduleId)
            .HasColumnName("schedule_id");

        builder.Property(x => x.LinkedAlertId)
            .HasColumnName("linked_alert_id");

        builder.Property(x => x.OrderType)
            .HasColumnName("order_type");

        builder.Property(x => x.Status)
            .HasColumnName("status");

        builder.Property(x => x.TriggerSource)
            .HasColumnName("trigger_source")
            .HasMaxLength(30);

        builder.Property(x => x.ExecutionDate)
            .HasColumnName("execution_date");

        builder.Property(x => x.LotoApplied)
            .HasColumnName("loto_applied");

        builder.Property(x => x.LotoAppliedAt)
            .HasColumnName("loto_applied_at");

        builder.Property(x => x.LotoTimeoutHours)
            .HasColumnName("loto_timeout_hours")
            .HasDefaultValue(24);

        builder.Property(x => x.ProtectorsReinstalled)
            .HasColumnName("protectors_reinstalled");

        builder.Property(x => x.ProtectorsVerifiedAt)
            .HasColumnName("protectors_verified_at");

        builder.Property(x => x.TechnicianNotes)
            .HasColumnName("technician_notes");

        builder.Property(x => x.PerformedByWorkerId)
            .HasColumnName("performed_by_worker_id");

        builder.Property(x => x.PartsUsed)
            .HasColumnName("parts_used")
            .HasColumnType("jsonb");

        builder.Property(x => x.LaborHours)
            .HasColumnName("labor_hours")
            .HasPrecision(5, 2);

        builder.Property(x => x.TotalCost)
            .HasColumnName("total_cost")
            .HasPrecision(15, 2);

        builder.Property(x => x.NextServiceDueAt)
            .HasColumnName("next_service_due_at");

        builder.Property(x => x.NextServiceDueHours)
            .HasColumnName("next_service_due_hours")
            .HasPrecision(10, 2);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => new { x.EquipmentId, x.ExecutionDate });
        builder.HasIndex(x => x.Status);
    }
}
