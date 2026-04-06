namespace RentMaq.Domain.Entities;

public class ExtraordinaryCharge
{
    public Guid ChargeId { get; set; }
    public Guid ContractId { get; set; }
    public Guid? DepositId { get; set; }
    public Guid? AssessmentId { get; set; }
    public Guid? CfdiId { get; set; }
    public string Status { get; set; } = null!; // DETECTED, ATTRIBUTED, APPLIED_TO_DEPOSIT, INVOICED
    public decimal Amount { get; set; }
    public decimal AmountFromDeposit { get; set; }
    public decimal AmountDirectBill { get; set; }
    public string Reason { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public RentalContract Contract { get; set; } = null!;
    public Deposit? Deposit { get; set; }
    public DamageAssessment? Assessment { get; set; }
    public CfdiDocument? Cfdi { get; set; }
}
