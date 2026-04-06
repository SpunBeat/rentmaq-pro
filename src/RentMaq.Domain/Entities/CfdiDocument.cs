namespace RentMaq.Domain.Entities;

public class CfdiDocument
{
    public Guid CfdiId { get; set; }
    public Guid? ContractId { get; set; }
    public string CfdiType { get; set; } = null!; // I, E, P
    public string PaymentMethod { get; set; } = null!; // PUE, PPD
    public Guid? UuidFiscal { get; set; }
    public decimal TotalAmount { get; set; }
    public string UsoCfdi { get; set; } = null!;
    public string? FormaPago { get; set; }
    public Guid? RelatedCfdiId { get; set; }
    public string? RelationType { get; set; }
    public string? CancelReason { get; set; }
    public string? CancellationStatus { get; set; }
    public string? XmlUrl { get; set; }
    public string? PdfUrl { get; set; }
    public string? PacProvider { get; set; }
    public string Status { get; set; } = null!; // TIMBRADO, CANCELADO, PENDIENTE_PAGO, PAGADO
    public DateTimeOffset IssuedAt { get; set; }

    // Navigation
    public RentalContract? Contract { get; set; }
    public CfdiDocument? RelatedCfdi { get; set; }
}
