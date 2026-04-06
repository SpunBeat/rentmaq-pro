namespace RentMaq.Domain.Entities;

public class EquipmentLoadProfile
{
    public Guid ProfileId { get; set; }
    public string EquipmentMake { get; set; } = null!;
    public string EquipmentModel { get; set; } = null!;
    public string ApplicationType { get; set; } = null!;
    public decimal EngineLoadFactor { get; set; }
    public decimal PtoLoadFactor { get; set; }
    public int? MaintenanceIntervalHours { get; set; }
    public decimal? TargetUtilizationPct { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}
