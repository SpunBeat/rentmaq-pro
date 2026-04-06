using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;

namespace RentMaq.Domain.Interfaces;

public interface IRentalContractRepository : IRepository<RentalContract>
{
    Task<IReadOnlyList<RentalContract>> GetByStatusAsync(ContractStatusEnum status, CancellationToken ct = default);
    Task<IReadOnlyList<RentalContract>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
}
