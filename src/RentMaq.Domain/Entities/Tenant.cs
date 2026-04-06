namespace RentMaq.Domain.Entities;

public class Tenant
{
    public Guid TenantId { get; set; }
    public string LegalName { get; set; } = null!;
    public string Rfc { get; set; } = null!;
    public string TaxRegime { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string UsoCfdiDefault { get; set; } = "G03";
    public decimal? CreditLimit { get; set; }
    public string CreditStatus { get; set; } = "PENDING_EVALUATION";
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public ICollection<RentalContract> Contracts { get; set; } = [];
}
