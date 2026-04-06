using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class DamageAssessment
{
    public Guid AssessmentId { get; set; }
    public Guid ContractId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid? FaultEventId { get; set; }
    public Guid? ChecklistId { get; set; }
    public Guid? WorkOrderId { get; set; }
    public DateTimeOffset InspectionDate { get; set; }
    public Guid AssessorId { get; set; }
    public string DamageDescription { get; set; } = null!;
    public DamageAttributionEnum Attribution { get; set; } = DamageAttributionEnum.UnderDispute;
    public decimal? EstimatedRepairCost { get; set; }
    public string? CustomerSignatureUrl { get; set; }
    public string? Photos { get; set; } // JSONB
    public string Status { get; set; } = null!; // DRAFT, UNDER_REVIEW, APPROVED_FOR_CHARGE, DISMISSED
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public RentalContract Contract { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
    public TelemetryAlert? FaultEvent { get; set; }
    public InspectionChecklist? Checklist { get; set; }
    public MaintenanceWorkOrder? WorkOrder { get; set; }
}
