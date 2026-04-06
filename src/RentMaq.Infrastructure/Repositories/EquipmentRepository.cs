using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class EquipmentRepository : Repository<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Equipment>> GetByStatusAsync(EquipmentStatusEnum status, CancellationToken ct = default)
        => await DbSet.Where(e => e.CurrentStatus == status).ToListAsync(ct);

    public async Task<IReadOnlyList<Equipment>> GetWithTelemetryEndpointAsync(CancellationToken ct = default)
        => await DbSet
            .Where(e => e.AempEndpointUrl != null
                && (e.CurrentStatus == EquipmentStatusEnum.Available || e.CurrentStatus == EquipmentStatusEnum.Rented))
            .ToListAsync(ct);
}
