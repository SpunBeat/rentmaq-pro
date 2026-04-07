using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class ExtraordinaryChargeRepository : Repository<ExtraordinaryCharge>, IExtraordinaryChargeRepository
{
    public ExtraordinaryChargeRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ExtraordinaryCharge>> GetByContractAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet.Where(ec => ec.ContractId == contractId).ToListAsync(ct);

    public async Task<IReadOnlyList<ExtraordinaryCharge>> GetPendingInvoicingAsync(CancellationToken ct = default)
        => await DbSet.Where(ec => ec.Status == "APPLIED_TO_DEPOSIT").ToListAsync(ct);
}
