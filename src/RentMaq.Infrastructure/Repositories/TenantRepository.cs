using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(RentMaqDbContext context) : base(context) { }

    public async Task<Tenant?> GetByRfcAsync(string rfc, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(t => t.Rfc == rfc, ct);
}
