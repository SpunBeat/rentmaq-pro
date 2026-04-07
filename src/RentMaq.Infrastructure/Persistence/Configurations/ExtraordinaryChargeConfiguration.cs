using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class ExtraordinaryChargeConfiguration : IEntityTypeConfiguration<ExtraordinaryCharge>
{
    public void Configure(EntityTypeBuilder<ExtraordinaryCharge> builder)
    {
        builder.ToTable("extraordinary_charges", t =>
        {
            t.HasCheckConstraint("chk_charge_status",
                "status IN ('DETECTED','ATTRIBUTED','APPLIED_TO_DEPOSIT','INVOICED')");
        });

        builder.HasKey(x => x.ChargeId);

        builder.Property(x => x.ChargeId)
            .HasColumnName("charge_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.DepositId)
            .HasColumnName("deposit_id");

        builder.Property(x => x.AssessmentId)
            .HasColumnName("assessment_id");

        builder.Property(x => x.CfdiId)
            .HasColumnName("cfdi_id");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30);

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(15, 2);

        builder.Property(x => x.AmountFromDeposit)
            .HasColumnName("amount_from_deposit")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.AmountDirectBill)
            .HasColumnName("amount_direct_bill")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.Reason)
            .HasColumnName("reason");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
