using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class DepositRepository : Repository<Deposit>, IDepositRepository
{
    public DepositRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Deposit>> GetByContractAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet.Where(d => d.ContractId == contractId).ToListAsync(ct);

    public async Task<decimal> GetAvailableBalanceAsync(Guid contractId, CancellationToken ct = default)
        => await DbSet
            .Where(d => d.ContractId == contractId && d.Status == "HELD_AS_LIABILITY")
            .SumAsync(d => d.Amount - d.AppliedAmount - d.RefundedAmount, ct);
}
