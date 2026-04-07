using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace RentMaq.Tests.Integration;

public class MigrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public MigrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migration_CreatesAllTables()
    {
        var tables = await _fixture.DbContext.Database
            .SqlQueryRaw<string>(
                "SELECT tablename AS \"Value\" FROM pg_tables WHERE schemaname = 'public' AND tablename != '__EFMigrationsHistory' ORDER BY tablename")
            .ToListAsync();

        tables.Should().Contain("equipment");
        tables.Should().Contain("telemetry_readings");
        tables.Should().Contain("telemetry_alerts");
        tables.Should().Contain("maintenance_work_orders");
        tables.Should().Contain("rental_contracts");
        tables.Should().Contain("cfdi_documents");
        tables.Should().Contain("deposits");
        tables.Should().Contain("damage_assessments");
        tables.Should().Contain("extraordinary_charges");
        tables.Should().Contain("geofences");
        tables.Should().Contain("tenants");
    }

    [Fact]
    public async Task Migration_CreatesPostgresEnums()
    {
        var enums = await _fixture.DbContext.Database
            .SqlQueryRaw<string>(
                "SELECT typname AS \"Value\" FROM pg_type WHERE typtype = 'e' AND typnamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public') ORDER BY typname")
            .ToListAsync();

        enums.Should().HaveCount(9);
        enums.Should().Contain("equipment_type_enum");
        enums.Should().Contain("equipment_status_enum");
        enums.Should().Contain("contract_status_enum");
        enums.Should().Contain("severity_enum");
    }

    [Fact]
    public async Task PostGIS_IsAvailable()
    {
        var result = await _fixture.DbContext.Database
            .SqlQueryRaw<string>("SELECT PostGIS_Version() AS \"Value\"")
            .FirstAsync();

        result.Should().StartWith("3.");
    }
}
