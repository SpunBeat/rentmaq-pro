using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class DamageAssessmentRepository : Repository<DamageAssessment>, IDamageAssessmentRepository
{
    public DamageAssessmentRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DamageAssessment>> GetPendingChargesByContractAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet
            .Where(da => da.ContractId == contractId && da.Status == "APPROVED_FOR_CHARGE")
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DamageAssessment>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default)
        => await DbSet.Where(da => da.EquipmentId == equipmentId).ToListAsync(ct);
}
