using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IMaintenanceScheduleRepository : IRepository<MaintenanceSchedule>
{
    Task<IReadOnlyList<MaintenanceSchedule>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceSchedule>> GetOverdueSchedulesAsync(decimal currentHours, DateOnly currentDate, CancellationToken ct = default);
}
