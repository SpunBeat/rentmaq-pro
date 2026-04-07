using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence.Configurations;

public class CfdiDocumentConfiguration : IEntityTypeConfiguration<CfdiDocument>
{
    public void Configure(EntityTypeBuilder<CfdiDocument> builder)
    {
        builder.ToTable("cfdi_documents", t =>
        {
            t.HasCheckConstraint("chk_cfdi_type",
                "cfdi_type IN ('I','E','P')");
            t.HasCheckConstraint("chk_payment_method",
                "payment_method IN ('PUE','PPD')");
            t.HasCheckConstraint("chk_cfdi_status",
                "status IN ('TIMBRADO','CANCELADO','PENDIENTE_PAGO','PAGADO')");
        });

        builder.HasKey(x => x.CfdiId);

        builder.Property(x => x.CfdiId)
            .HasColumnName("cfdi_id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id");

        builder.Property(x => x.CfdiType)
            .HasColumnName("cfdi_type")
            .HasMaxLength(1);

        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(3);

        builder.Property(x => x.UuidFiscal)
            .HasColumnName("uuid_fiscal");

        builder.Property(x => x.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(15, 2);

        builder.Property(x => x.UsoCfdi)
            .HasColumnName("uso_cfdi")
            .HasMaxLength(4);

        builder.Property(x => x.FormaPago)
            .HasColumnName("forma_pago")
            .HasMaxLength(2);

        builder.Property(x => x.RelatedCfdiId)
            .HasColumnName("related_cfdi_id");

        builder.Property(x => x.RelationType)
            .HasColumnName("relation_type")
            .HasMaxLength(2);

        builder.Property(x => x.CancelReason)
            .HasColumnName("cancel_reason")
            .HasMaxLength(2);

        builder.Property(x => x.CancellationStatus)
            .HasColumnName("cancellation_status")
            .HasMaxLength(50);

        builder.Property(x => x.XmlUrl)
            .HasColumnName("xml_url")
            .HasMaxLength(500);

        builder.Property(x => x.PdfUrl)
            .HasColumnName("pdf_url")
            .HasMaxLength(500);

        builder.Property(x => x.PacProvider)
            .HasColumnName("pac_provider")
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20);

        builder.Property(x => x.IssuedAt)
            .HasColumnName("issued_at")
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.UuidFiscal).IsUnique();
    }
}
