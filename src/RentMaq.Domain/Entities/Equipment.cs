using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class Equipment
{
    public Guid EquipmentId { get; set; }
    public string AssetTag { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int? Year { get; set; }
    public EquipmentTypeEnum EquipmentType { get; set; }
    public decimal? WeightTons { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public EquipmentStatusEnum CurrentStatus { get; set; } = EquipmentStatusEnum.Available;
    public string? AempEndpointUrl { get; set; }
    public Guid? LoadProfileId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public EquipmentLoadProfile? LoadProfile { get; set; }
    public ICollection<TelemetryReading> TelemetryReadings { get; set; } = [];
    public ICollection<TelemetryAlert> TelemetryAlerts { get; set; } = [];
    public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = [];
    public ICollection<MaintenanceWorkOrder> WorkOrders { get; set; } = [];
    public ICollection<EquipmentCertification> Certifications { get; set; } = [];
}
