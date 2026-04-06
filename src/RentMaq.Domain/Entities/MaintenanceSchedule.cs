namespace RentMaq.Domain.Entities;

public class MaintenanceSchedule
{
    public Guid ScheduleId { get; set; }
    public Guid EquipmentId { get; set; }
    public string ServiceTier { get; set; } = null!;
    public int? IntervalHours { get; set; }
    public int? IntervalDays { get; set; }
    public decimal? LastServiceHours { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public decimal? NextDueHours { get; set; }
    public DateOnly? NextDueDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
}
