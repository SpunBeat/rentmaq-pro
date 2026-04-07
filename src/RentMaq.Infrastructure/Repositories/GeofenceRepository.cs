using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;

namespace RentMaq.Infrastructure.Repositories;

public class GeofenceRepository : Repository<Geofence>, IGeofenceRepository
{
    public GeofenceRepository(RentMaqDbContext context) : base(context) { }

    public async Task<Geofence?> GetByNameAsync(string name, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(g => g.Name == name, ct);
}
