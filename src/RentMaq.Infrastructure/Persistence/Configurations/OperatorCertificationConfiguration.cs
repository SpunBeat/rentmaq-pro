using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class OperatorCertificationConfiguration : IEntityTypeConfiguration<OperatorCertification>
{
    public void Configure(EntityTypeBuilder<OperatorCertification> builder)
    {
        builder.ToTable("operator_certifications");

        builder.HasKey(x => x.OperatorCertId);

        builder.Property(x => x.OperatorCertId)
            .HasColumnName("operator_cert_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.OperatorName)
            .HasColumnName("operator_name")
            .HasMaxLength(200);

        builder.Property(x => x.Dc3CertificateNumber)
            .HasColumnName("dc3_certificate_number")
            .HasMaxLength(100);

        builder.Property(x => x.EquipmentTypeCertified)
            .HasColumnName("equipment_type_certified");

        builder.Property(x => x.IssuedAt)
            .HasColumnName("issued_at");

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(x => x.DocumentUrl)
            .HasColumnName("document_url")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
