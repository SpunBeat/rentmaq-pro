using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class EquipmentCertificationConfiguration : IEntityTypeConfiguration<EquipmentCertification>
{
    public void Configure(EntityTypeBuilder<EquipmentCertification> builder)
    {
        builder.ToTable("equipment_certifications", t =>
        {
            t.HasCheckConstraint("chk_cert_status",
                "status IN ('VALID','EXPIRED','REVOKED')");
        });

        builder.HasKey(x => x.CertificationId);

        builder.Property(x => x.CertificationId)
            .HasColumnName("certification_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(x => x.CertificationName)
            .HasColumnName("certification_name")
            .HasMaxLength(100);

        builder.Property(x => x.CertificationType)
            .HasColumnName("certification_type")
            .HasMaxLength(50);

        builder.Property(x => x.IssuedBy)
            .HasColumnName("issued_by")
            .HasMaxLength(200);

        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date");

        builder.Property(x => x.ExpirationDate)
            .HasColumnName("expiration_date");

        builder.Property(x => x.BlocksRental)
            .HasColumnName("blocks_rental")
            .HasDefaultValue(true);

        builder.Property(x => x.DocumentUrl)
            .HasColumnName("document_url")
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");
    }
}
