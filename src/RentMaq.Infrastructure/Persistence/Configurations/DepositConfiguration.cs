using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class DepositConfiguration : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> builder)
    {
        builder.ToTable("deposits", t =>
        {
            t.HasCheckConstraint("chk_deposit_balance",
                "applied_amount + refunded_amount <= amount");
            t.HasCheckConstraint("chk_deposit_status",
                "status IN ('PENDING_COLLECTION','HELD_AS_LIABILITY','PARTIALLY_APPLIED','FULLY_APPLIED','REFUNDED')");
            t.HasCheckConstraint("chk_accounting_class",
                "accounting_classification IN ('LIABILITY','RECOGNIZED_INCOME')");
        });

        builder.HasKey(x => x.DepositId);

        builder.Property(x => x.DepositId)
            .HasColumnName("deposit_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(15, 2);

        builder.Property(x => x.AppliedAmount)
            .HasColumnName("applied_amount")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.RefundedAmount)
            .HasColumnName("refunded_amount")
            .HasPrecision(15, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.RelatedCfdiId)
            .HasColumnName("related_cfdi_id");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30);

        builder.Property(x => x.AccountingClassification)
            .HasColumnName("accounting_classification")
            .HasMaxLength(20)
            .HasDefaultValue("LIABILITY");

        builder.Property(x => x.ReceivedAt)
            .HasColumnName("received_at");
    }
}
