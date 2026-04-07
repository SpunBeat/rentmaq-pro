using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentMaq.Domain.Entities;

namespace RentMaq.Tests.Integration;

public class GeospatialTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public GeospatialTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Geofence_CanStoreAndRetrieve_Polygon()
    {
        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        var polygon = factory.CreatePolygon(new[]
        {
            new Coordinate(-99.17, 19.43),
            new Coordinate(-99.16, 19.43),
            new Coordinate(-99.16, 19.44),
            new Coordinate(-99.17, 19.44),
            new Coordinate(-99.17, 19.43)
        });

        var geofence = new Geofence
        {
            GeofenceId = Guid.NewGuid(),
            Name = "Obra CDMX Norte",
            Perimeter = polygon,
            IsYard = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _fixture.DbContext.Geofences.Add(geofence);
        await _fixture.DbContext.SaveChangesAsync();

        _fixture.DbContext.ChangeTracker.Clear();

        var retrieved = await _fixture.DbContext.Geofences
            .FirstOrDefaultAsync(g => g.GeofenceId == geofence.GeofenceId);

        retrieved.Should().NotBeNull();
        retrieved!.Perimeter.Should().NotBeNull();
        retrieved.Perimeter.GeometryType.Should().Be("Polygon");
    }

    [Fact]
    public async Task ST_Covers_DetectsPointInsideGeofence()
    {
        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        var polygon = factory.CreatePolygon(new[]
        {
            new Coordinate(-99.20, 19.40),
            new Coordinate(-99.10, 19.40),
            new Coordinate(-99.10, 19.50),
            new Coordinate(-99.20, 19.50),
            new Coordinate(-99.20, 19.40)
        });

        var geofence = new Geofence
        {
            GeofenceId = Guid.NewGuid(),
            Name = "Obra Test ST_Covers",
            Perimeter = polygon,
            IsYard = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _fixture.DbContext.Geofences.Add(geofence);
        await _fixture.DbContext.SaveChangesAsync();

        // Punto dentro del poligono
        var pointInside = factory.CreatePoint(new Coordinate(-99.15, 19.45));

        var result = await _fixture.DbContext.Geofences
            .Where(g => g.Perimeter.Covers(pointInside))
            .AnyAsync();

        result.Should().BeTrue("el punto esta dentro del poligono");

        // Punto fuera del poligono
        var pointOutside = factory.CreatePoint(new Coordinate(-99.00, 19.00));

        var resultOutside = await _fixture.DbContext.Geofences
            .Where(g => g.Perimeter.Covers(pointOutside))
            .AnyAsync();

        resultOutside.Should().BeFalse("el punto esta fuera del poligono");
    }
}
