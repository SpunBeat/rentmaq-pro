using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IDepositRepository : IRepository<Deposit>
{
    Task<IReadOnlyList<Deposit>> GetByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<decimal> GetAvailableBalanceAsync(Guid contractId, CancellationToken ct = default);
}
