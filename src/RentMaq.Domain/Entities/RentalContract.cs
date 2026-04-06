using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class RentalContract
{
    public Guid ContractId { get; set; }
    public Guid TenantId { get; set; }
    public Guid EquipmentId { get; set; }
    public ContractStatusEnum Status { get; set; } = ContractStatusEnum.Draft;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    // Billing
    public BillingModelEnum BillingModel { get; set; } = BillingModelEnum.FixedPeriod;
    public decimal BaseMonthlyRate { get; set; }
    public decimal? DailyRate { get; set; }
    public decimal? WeeklyRate { get; set; }
    public decimal? OvertimeMultiplier { get; set; }
    public int? MaxHoursIncluded { get; set; }

    // Deposit & Insurance
    public string? DepositType { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    // Geofence
    public Guid? GeofenceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
    public Geofence? Geofence { get; set; }
    public ICollection<Deposit> Deposits { get; set; } = [];
    public ICollection<CfdiDocument> CfdiDocuments { get; set; } = [];
    public ICollection<DamageAssessment> DamageAssessments { get; set; } = [];
    public ICollection<ExtraordinaryCharge> ExtraordinaryCharges { get; set; } = [];
    public ICollection<OperatorCertification> OperatorCertifications { get; set; } = [];
    public ICollection<InspectionChecklist> InspectionChecklists { get; set; } = [];
}
