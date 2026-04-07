using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IDamageAssessmentRepository : IRepository<DamageAssessment>
{
    Task<IReadOnlyList<DamageAssessment>> GetPendingChargesByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<DamageAssessment>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default);
}
