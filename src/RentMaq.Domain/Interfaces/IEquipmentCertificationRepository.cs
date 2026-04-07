using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IEquipmentCertificationRepository : IRepository<EquipmentCertification>
{
    Task<IReadOnlyList<EquipmentCertification>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default);
    Task<IReadOnlyList<EquipmentCertification>> GetExpiringCertificationsAsync(int daysAhead, CancellationToken ct = default);
}
