using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class EquipmentCertificationRepository : Repository<EquipmentCertification>, IEquipmentCertificationRepository
{
    public EquipmentCertificationRepository(RentMaqDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EquipmentCertification>> GetByEquipmentAsync(Guid equipmentId, CancellationToken ct = default)
        => await DbSet.Where(ec => ec.EquipmentId == equipmentId).ToListAsync(ct);

    public async Task<IReadOnlyList<EquipmentCertification>> GetExpiringCertificationsAsync(int daysAhead, CancellationToken ct = default)
    {
        var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(daysAhead);
        return await DbSet
            .Where(ec => ec.ExpirationDate <= cutoffDate && ec.Status == "VALID")
            .ToListAsync(ct);
    }
}
