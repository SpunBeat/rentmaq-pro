using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class EquipmentThreshold
{
    public Guid ThresholdId { get; set; }
    public EquipmentTypeEnum EquipmentType { get; set; }
    public string? EquipmentModel { get; set; }
    public string ThresholdType { get; set; } = null!;
    public decimal? WarningValue { get; set; }
    public decimal? CriticalValue { get; set; }
    public decimal? ShutdownValue { get; set; }
    public string UnitOfMeasure { get; set; } = null!;
}
