using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class MaintenanceWorkOrderRepository : Repository<MaintenanceWorkOrder>, IMaintenanceWorkOrderRepository
{
    public MaintenanceWorkOrderRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<MaintenanceWorkOrder>> GetPendingByEquipmentAsync(Guid equipmentId, CancellationToken ct = default)
        => await DbSet
            .Where(wo => wo.EquipmentId == equipmentId && wo.Status != WorkOrderStatusEnum.Completed && wo.Status != WorkOrderStatusEnum.Cancelled)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MaintenanceWorkOrder>> GetByStatusAsync(WorkOrderStatusEnum status, CancellationToken ct = default)
        => await DbSet.Where(wo => wo.Status == status).ToListAsync(ct);
}
