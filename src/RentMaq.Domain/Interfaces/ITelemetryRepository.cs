using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface ITelemetryRepository : IRepository<TelemetryReading>
{
    Task<IReadOnlyList<TelemetryReading>> GetByEquipmentAsync(Guid equipmentId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task AddBatchAsync(IEnumerable<TelemetryReading> readings, CancellationToken ct = default);
}
