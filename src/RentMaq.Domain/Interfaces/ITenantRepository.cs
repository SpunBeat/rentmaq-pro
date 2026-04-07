using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByRfcAsync(string rfc, CancellationToken ct = default);
}
