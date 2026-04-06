namespace RentMaq.Domain.Entities;

public class Deposit
{
    public Guid DepositId { get; set; }
    public Guid ContractId { get; set; }
    public decimal Amount { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public Guid? RelatedCfdiId { get; set; }
    public string Status { get; set; } = null!; // PENDING_COLLECTION, HELD_AS_LIABILITY, etc.
    public string AccountingClassification { get; set; } = "LIABILITY";
    public DateTimeOffset? ReceivedAt { get; set; }

    // Navigation
    public RentalContract Contract { get; set; } = null!;
    public CfdiDocument? RelatedCfdi { get; set; }
}
