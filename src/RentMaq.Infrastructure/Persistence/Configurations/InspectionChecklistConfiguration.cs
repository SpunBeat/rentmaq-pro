using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class InspectionChecklistConfiguration : IEntityTypeConfiguration<InspectionChecklist>
{
    public void Configure(EntityTypeBuilder<InspectionChecklist> builder)
    {
        builder.ToTable("inspection_checklists", t =>
        {
            t.HasCheckConstraint("chk_inspection_type",
                "inspection_type IN ('PRE_DELIVERY','POST_RETURN')");
            t.HasCheckConstraint("chk_overall_result",
                "overall_result IN ('APPROVED','APPROVED_WITH_OBSERVATIONS','REJECTED')");
        });

        builder.HasKey(x => x.ChecklistId);

        builder.Property(x => x.ChecklistId)
            .HasColumnName("checklist_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.InspectionType)
            .HasColumnName("inspection_type")
            .HasMaxLength(20);

        builder.Property(x => x.InspectorId)
            .HasColumnName("inspector_id");

        builder.Property(x => x.ChecklistItems)
            .HasColumnName("checklist_items")
            .HasColumnType("jsonb");

        builder.Property(x => x.Photos)
            .HasColumnName("photos")
            .HasColumnType("jsonb");

        builder.Property(x => x.OverallResult)
            .HasColumnName("overall_result")
            .HasMaxLength(30);

        builder.Property(x => x.HorometerReading)
            .HasColumnName("horometer_reading")
            .HasPrecision(10, 2);

        builder.Property(x => x.FuelLevelPct)
            .HasColumnName("fuel_level_pct")
            .HasPrecision(5, 2);

        builder.Property(x => x.InspectedAt)
            .HasColumnName("inspected_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
