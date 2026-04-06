using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class RentalContractRepository : Repository<RentalContract>, IRentalContractRepository
{
    public RentalContractRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<RentalContract>> GetByStatusAsync(ContractStatusEnum status, CancellationToken ct = default)
        => await DbSet.Where(c => c.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<RentalContract>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await DbSet.Where(c => c.TenantId == tenantId).ToListAsync(ct);
}
