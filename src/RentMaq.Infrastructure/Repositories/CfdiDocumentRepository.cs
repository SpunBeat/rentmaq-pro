using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class CfdiDocumentRepository : Repository<CfdiDocument>, ICfdiDocumentRepository
{
    public CfdiDocumentRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CfdiDocument>> GetByContractAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet.Where(c => c.ContractId == contractId).ToListAsync(ct);

    public async Task<CfdiDocument?> GetByUuidFiscalAsync(Guid uuidFiscal, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.UuidFiscal == uuidFiscal, ct);
}
