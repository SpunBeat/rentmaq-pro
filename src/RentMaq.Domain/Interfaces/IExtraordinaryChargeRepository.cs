using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IExtraordinaryChargeRepository : IRepository<ExtraordinaryCharge>
{
    Task<IReadOnlyList<ExtraordinaryCharge>> GetByContractAsync(Guid contractId, CancellationToken ct = default);
    Task<IReadOnlyList<ExtraordinaryCharge>> GetPendingInvoicingAsync(CancellationToken ct = default);
}
