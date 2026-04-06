using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Interfaces;

public interface IMaintenanceWorkOrderRepository : IRepository<MaintenanceWorkOrder>
{
    Task<IReadOnlyList<MaintenanceWorkOrder>> GetPendingByEquipmentAsync(Guid equipmentId, CancellationToken ct = default);
    Task<IReadOnlyList<MaintenanceWorkOrder>> GetByStatusAsync(WorkOrderStatusEnum status, CancellationToken ct = default);
}
