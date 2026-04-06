using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Entities;

public class MaintenanceWorkOrder
{
    public Guid WorkOrderId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid? ScheduleId { get; set; }
    public Guid? LinkedAlertId { get; set; }

    public MaintenanceOrderTypeEnum OrderType { get; set; }
    public WorkOrderStatusEnum Status { get; set; } = WorkOrderStatusEnum.Pending;
    public string TriggerSource { get; set; } = null!;
    public DateOnly ExecutionDate { get; set; }

    // LOTO — NOM-004 Art. 7.2.2 c)
    public bool LotoApplied { get; set; }
    public DateTimeOffset? LotoAppliedAt { get; set; }
    public int LotoTimeoutHours { get; set; } = 24;

    // Protectors — NOM-004 Art. 7.2.2 a)
    public bool ProtectorsReinstalled { get; set; }
    public DateTimeOffset? ProtectorsVerifiedAt { get; set; }

    public string? TechnicianNotes { get; set; }
    public Guid? PerformedByWorkerId { get; set; }
    public string? PartsUsed { get; set; } // JSONB
    public decimal? LaborHours { get; set; }
    public decimal? TotalCost { get; set; }
    public DateOnly? NextServiceDueAt { get; set; }
    public decimal? NextServiceDueHours { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public Equipment Equipment { get; set; } = null!;
    public MaintenanceSchedule? Schedule { get; set; }
    public TelemetryAlert? LinkedAlert { get; set; }
}
