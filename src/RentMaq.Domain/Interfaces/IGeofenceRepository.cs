using RentMaq.Domain.Entities;

namespace RentMaq.Domain.Interfaces;

public interface IGeofenceRepository : IRepository<Geofence>
{
    Task<Geofence?> GetByNameAsync(string name, CancellationToken ct = default);
}
