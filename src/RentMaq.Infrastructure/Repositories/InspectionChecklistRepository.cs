using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class InspectionChecklistRepository : Repository<InspectionChecklist>, IInspectionChecklistRepository
{
    public InspectionChecklistRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InspectionChecklist>> GetByContractAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet.Where(ic => ic.ContractId == contractId).ToListAsync(ct);

    public async Task<IReadOnlyList<InspectionChecklist>> GetByEquipmentAndTypeAsync(
        Guid equipmentId, string inspectionType, CancellationToken ct = default)
        => await DbSet
            .Where(ic => ic.EquipmentId == equipmentId && ic.InspectionType == inspectionType)
            .ToListAsync(ct);
}
