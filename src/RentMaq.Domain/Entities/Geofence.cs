namespace RentMaq.Domain.Entities;

/// <summary>
/// Perimeter stored as GEOGRAPHY(POLYGON, 4326) in PostgreSQL.
/// The Perimeter property holds the WKT or is mapped via NetTopologySuite.
/// </summary>
public class Geofence
{
    public Guid GeofenceId { get; set; }
    public string Name { get; set; } = null!;
    // Mapped via NetTopologySuite Geometry in Infrastructure
    public NetTopologySuite.Geometries.Geometry Perimeter { get; set; } = null!;
    public bool IsYard { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
