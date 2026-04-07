using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class DamageAssessmentConfiguration : IEntityTypeConfiguration<DamageAssessment>
{
    public void Configure(EntityTypeBuilder<DamageAssessment> builder)
    {
        builder.ToTable("damage_assessments", t =>
        {
            t.HasCheckConstraint("chk_damage_status",
                "status IN ('DRAFT','UNDER_REVIEW','APPROVED_FOR_CHARGE','DISMISSED')");
        });

        builder.HasKey(x => x.AssessmentId);

        builder.Property(x => x.AssessmentId)
            .HasColumnName("assessment_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.FaultEventId)
            .HasColumnName("fault_event_id");

        builder.Property(x => x.ChecklistId)
            .HasColumnName("checklist_id");

        builder.Property(x => x.WorkOrderId)
            .HasColumnName("work_order_id");

        builder.Property(x => x.InspectionDate)
            .HasColumnName("inspection_date");

        builder.Property(x => x.AssessorId)
            .HasColumnName("assessor_id");

        builder.Property(x => x.DamageDescription)
            .HasColumnName("damage_description");

        builder.Property(x => x.Attribution)
            .HasColumnName("attribution");

        builder.Property(x => x.EstimatedRepairCost)
            .HasColumnName("estimated_repair_cost")
            .HasPrecision(15, 2);

        builder.Property(x => x.CustomerSignatureUrl)
            .HasColumnName("customer_signature_url")
            .HasMaxLength(500);

        builder.Property(x => x.Photos)
            .HasColumnName("photos")
            .HasColumnType("jsonb");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");
    }
}
