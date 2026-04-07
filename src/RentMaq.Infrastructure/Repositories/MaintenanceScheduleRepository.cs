using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class MaintenanceScheduleRepository : Repository<MaintenanceSchedule>, IMaintenanceScheduleRepository
{
    public MaintenanceScheduleRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MaintenanceSchedule>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default)
        => await DbSet.Where(ms => ms.EquipmentId == equipmentId).ToListAsync(ct);

    public async Task<IReadOnlyList<MaintenanceSchedule>> GetOverdueSchedulesAsync(
        decimal currentHours, DateOnly currentDate, CancellationToken ct = default)
        => await DbSet
            .Where(ms => ms.IsActive
                && ((ms.NextDueHours != null && ms.NextDueHours <= currentHours)
                    || (ms.NextDueDate != null && ms.NextDueDate <= currentDate)))
            .ToListAsync(ct);
}
