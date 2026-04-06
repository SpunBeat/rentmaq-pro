namespace RentMaq.Domain.Entities;

public class InspectionChecklist
{
    public Guid ChecklistId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid? ContractId { get; set; }
    public string InspectionType { get; set; } = null!; // PRE_DELIVERY, POST_RETURN
    public Guid InspectorId { get; set; }
    public string ChecklistItems { get; set; } = null!; // JSONB
    public string? Photos { get; set; } // JSONB
    public string OverallResult { get; set; } = null!; // APPROVED, APPROVED_WITH_OBSERVATIONS, REJECTED
    public decimal? HorometerReading { get; set; }
    public decimal? FuelLevelPct { get; set; }
    public DateTimeOffset InspectedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
    public RentalContract? Contract { get; set; }
}
