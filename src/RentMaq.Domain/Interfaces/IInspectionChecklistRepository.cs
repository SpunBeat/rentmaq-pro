using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IInspectionChecklistRepository : IRepository<InspectionChecklist>
{
    Task<IReadOnlyList<InspectionChecklist>> GetByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionChecklist>> GetByEquipmentAndTypeAsync(Guid equipmentId, string inspectionType, CancellationToken ct = default);
}
