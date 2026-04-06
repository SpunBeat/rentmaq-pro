using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Interfaces;

public interface IEquipmentRepository : IRepository<Equipment>
{
    Task<IReadOnlyList<Equipment>> GetByStatusAsync(EquipmentStatusEnum status, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetWithTelemetryEndpointAsync(CancellationToken ct = default);
}
