using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class RentalContractConfiguration : IEntityTypeConfiguration<RentalContract>
{
    public void Configure(EntityTypeBuilder<RentalContract> builder)
    {
        builder.ToTable("rental_contracts");

        builder.HasKey(x => x.ContractId);

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.Status)
            .HasColumnName("status");

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date");

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date");

        builder.Property(x => x.BillingModel)
            .HasColumnName("billing_model");

        builder.Property(x => x.BaseMonthlyRate)
            .HasColumnName("base_monthly_rate")
            .HasPrecision(15, 2);

        builder.Property(x => x.DailyRate)
            .HasColumnName("daily_rate")
            .HasPrecision(15, 2);

        builder.Property(x => x.WeeklyRate)
            .HasColumnName("weekly_rate")
            .HasPrecision(15, 2);

        builder.Property(x => x.OvertimeMultiplier)
            .HasColumnName("overtime_multiplier")
            .HasPrecision(5, 2);

        builder.Property(x => x.MaxHoursIncluded)
            .HasColumnName("max_hours_included");

        builder.Property(x => x.DepositType)
            .HasColumnName("deposit_type")
            .HasMaxLength(50);

        builder.Property(x => x.InsurancePolicyNumber)
            .HasColumnName("insurance_policy_number")
            .HasMaxLength(100);

        builder.Property(x => x.GeofenceId)
            .HasColumnName("geofence_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("NOW()");
    }
}
