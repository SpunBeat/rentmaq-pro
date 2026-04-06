using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class TelemetryRepository : Repository<TelemetryReading>, ITelemetryRepository
{
    public TelemetryRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TelemetryReading>> GetByEquipmentAsync(
        Guid equipmentId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await DbSet
            .Where(r => r.EquipmentId == equipmentId && r.RecordedAt >= from && r.RecordedAt <= to)
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(ct);

    public async Task AddBatchAsync(IEnumerable<TelemetryReading> readings, CancellationToken ct = default)
        => await DbSet.AddRangeAsync(readings, ct);
}
