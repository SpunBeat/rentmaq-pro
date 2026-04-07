using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", t =>
        {
            t.HasCheckConstraint("chk_credit_status",
                "credit_status IN ('PENDING_EVALUATION','APPROVED','SUSPENDED','BLOCKED')");
        });

        builder.HasKey(x => x.TenantId);

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(255);

        builder.Property(x => x.Rfc)
            .HasColumnName("rfc")
            .HasMaxLength(13);

        builder.Property(x => x.TaxRegime)
            .HasColumnName("tax_regime")
            .HasMaxLength(3);

        builder.Property(x => x.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(5);

        builder.Property(x => x.UsoCfdiDefault)
            .HasColumnName("uso_cfdi_default")
            .HasMaxLength(4)
            .HasDefaultValue("G03");

        builder.Property(x => x.CreditLimit)
            .HasColumnName("credit_limit")
            .HasPrecision(15, 2);

        builder.Property(x => x.CreditStatus)
            .HasColumnName("credit_status")
            .HasMaxLength(30)
            .HasDefaultValue("PENDING_EVALUATION");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.Rfc).IsUnique();
    }
}
