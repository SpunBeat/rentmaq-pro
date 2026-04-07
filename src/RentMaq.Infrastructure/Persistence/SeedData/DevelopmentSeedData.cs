using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RentMaq.Domain.Entities;
using RentMaq.Domain.Enums;

namespace RentMaq.Infrastructure.Persistence.SeedData;

public static class DevelopmentSeedData
{
    public static async Task SeedAsync(RentMaqDbContext context)
    {
        if (await context.Equipment.AnyAsync())
            return;

        var factory = new GeometryFactory(new PrecisionModel(), 4326);

        // --- Equipment Load Profiles ---
        var profileMiniExc = new EquipmentLoadProfile
        {
            ProfileId = Guid.NewGuid(),
            EquipmentMake = "Caterpillar",
            EquipmentModel = "303.5E2",
            ApplicationType = "excavacion",
            EngineLoadFactor = 0.650m,
            PtoLoadFactor = 0.300m,
            MaintenanceIntervalHours = 250,
            TargetUtilizationPct = 75.00m,
            LastUpdated = DateTimeOffset.UtcNow
        };
        var profileScissor = new EquipmentLoadProfile
        {
            ProfileId = Guid.NewGuid(),
            EquipmentMake = "JLG",
            EquipmentModel = "3246ES",
            ApplicationType = "elevacion",
            EngineLoadFactor = 0.350m,
            PtoLoadFactor = 0.100m,
            MaintenanceIntervalHours = 500,
            TargetUtilizationPct = 60.00m,
            LastUpdated = DateTimeOffset.UtcNow
        };
        var profileCompactor = new EquipmentLoadProfile
        {
            ProfileId = Guid.NewGuid(),
            EquipmentMake = "Wacker Neuson",
            EquipmentModel = "DPU6555",
            ApplicationType = "compactacion",
            EngineLoadFactor = 0.800m,
            PtoLoadFactor = 0.750m,
            MaintenanceIntervalHours = 200,
            TargetUtilizationPct = 85.00m,
            LastUpdated = DateTimeOffset.UtcNow
        };

        context.EquipmentLoadProfiles.AddRange(profileMiniExc, profileScissor, profileCompactor);

        // --- Equipment (5 equipos) ---
        var eq1 = new Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = "RMQ-001",
            SerialNumber = "CAT303E2-2024-0001",
            Make = "Caterpillar",
            Model = "303.5E2",
            Year = 2024,
            EquipmentType = EquipmentTypeEnum.Miniexcavator,
            WeightTons = 3.78m,
            AcquisitionCost = 850_000.00m,
            AcquisitionDate = new DateOnly(2024, 3, 15),
            CurrentStatus = EquipmentStatusEnum.Available,
            AempEndpointUrl = "https://aemp.cat.com/api/v2/fleet/303E2-0001",
            LoadProfileId = profileMiniExc.ProfileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var eq2 = new Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = "RMQ-002",
            SerialNumber = "JLG3246ES-2023-0042",
            Make = "JLG",
            Model = "3246ES",
            Year = 2023,
            EquipmentType = EquipmentTypeEnum.ScissorLift,
            WeightTons = 2.90m,
            AcquisitionCost = 420_000.00m,
            AcquisitionDate = new DateOnly(2023, 8, 1),
            CurrentStatus = EquipmentStatusEnum.Rented,
            AempEndpointUrl = "https://aemp.jlg.com/api/v2/fleet/3246ES-0042",
            LoadProfileId = profileScissor.ProfileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var eq3 = new Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = "RMQ-003",
            SerialNumber = "WN-DPU6555-2024-0015",
            Make = "Wacker Neuson",
            Model = "DPU6555",
            Year = 2024,
            EquipmentType = EquipmentTypeEnum.Compactor,
            WeightTons = 0.45m,
            AcquisitionCost = 180_000.00m,
            CurrentStatus = EquipmentStatusEnum.Available,
            LoadProfileId = profileCompactor.ProfileId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var eq4 = new Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = "RMQ-004",
            SerialNumber = "MULTIQUIP-DCA25-0008",
            Make = "Multiquip",
            Model = "DCA25SSIU4F",
            Year = 2024,
            EquipmentType = EquipmentTypeEnum.Generator,
            WeightTons = 0.85m,
            AcquisitionCost = 320_000.00m,
            CurrentStatus = EquipmentStatusEnum.Available,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var eq5 = new Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = "RMQ-005",
            SerialNumber = "GENIE-S45-2023-0033",
            Make = "Genie",
            Model = "S-45",
            Year = 2023,
            EquipmentType = EquipmentTypeEnum.BoomLift,
            WeightTons = 7.17m,
            AcquisitionCost = 1_200_000.00m,
            CurrentStatus = EquipmentStatusEnum.Available,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Equipment.AddRange(eq1, eq2, eq3, eq4, eq5);

        // --- Equipment Thresholds ---
        var thresholds = new[]
        {
            // Plataforma de tijera: umbrales estrictos de inclinacion (NOM-009)
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.ScissorLift, ThresholdType = "tilt_lateral", WarningValue = 1.5m, CriticalValue = 2.0m, ShutdownValue = 3.0m, UnitOfMeasure = "degrees" },
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.ScissorLift, ThresholdType = "impact_g", WarningValue = 0.8m, CriticalValue = 1.5m, ShutdownValue = 2.5m, UnitOfMeasure = "g" },
            // Compactador: umbrales altos de vibracion (operacion normal genera altas vibraciones)
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.Compactor, ThresholdType = "impact_g", WarningValue = 3.0m, CriticalValue = 5.0m, ShutdownValue = 8.0m, UnitOfMeasure = "g" },
            // Miniexcavadora
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.Miniexcavator, ThresholdType = "hydraulic_psi", WarningValue = 3000.000m, CriticalValue = 3500.000m, ShutdownValue = 4000.000m, UnitOfMeasure = "psi" },
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.Miniexcavator, ThresholdType = "coolant_temp", WarningValue = 95.000m, CriticalValue = 105.000m, ShutdownValue = 115.000m, UnitOfMeasure = "celsius" },
            // Generador
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.Generator, ThresholdType = "coolant_temp", WarningValue = 90.000m, CriticalValue = 100.000m, ShutdownValue = 110.000m, UnitOfMeasure = "celsius" },
            // Boom lift
            new EquipmentThreshold { ThresholdId = Guid.NewGuid(), EquipmentType = EquipmentTypeEnum.BoomLift, ThresholdType = "tilt_lateral", WarningValue = 2.0m, CriticalValue = 3.0m, ShutdownValue = 5.0m, UnitOfMeasure = "degrees" },
        };
        context.EquipmentThresholds.AddRange(thresholds);

        // --- Geofence ---
        var geofence = new Geofence
        {
            GeofenceId = Guid.NewGuid(),
            Name = "Obra Reforma 222 - CDMX",
            Perimeter = factory.CreatePolygon(new[]
            {
                new Coordinate(-99.1720, 19.4270),
                new Coordinate(-99.1680, 19.4270),
                new Coordinate(-99.1680, 19.4310),
                new Coordinate(-99.1720, 19.4310),
                new Coordinate(-99.1720, 19.4270)
            }),
            IsYard = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Geofences.Add(geofence);

        // --- Tenants (RFCs de prueba del SAT) ---
        var tenant1 = new Tenant
        {
            TenantId = Guid.NewGuid(),
            LegalName = "Construcciones del Valle SA de CV",
            Rfc = "CVA200315AB1",
            TaxRegime = "601",
            PostalCode = "06600",
            UsoCfdiDefault = "G03",
            CreditLimit = 500_000.00m,
            CreditStatus = "APPROVED",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var tenant2 = new Tenant
        {
            TenantId = Guid.NewGuid(),
            LegalName = "Infraestructura Norte SA de CV",
            Rfc = "INO190822CK3",
            TaxRegime = "601",
            PostalCode = "64000",
            UsoCfdiDefault = "I01",
            CreditLimit = 1_000_000.00m,
            CreditStatus = "APPROVED",
            CreatedAt = DateTimeOffset.UtcNow
        };
        var tenant3 = new Tenant
        {
            TenantId = Guid.NewGuid(),
            LegalName = "Desarrollos Urbanos Poniente SA de CV",
            Rfc = "DUP210610MN5",
            TaxRegime = "601",
            PostalCode = "45050",
            CreditStatus = "PENDING_EVALUATION",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Tenants.AddRange(tenant1, tenant2, tenant3);

        // --- Rental Contracts ---
        var contract1 = new RentalContract
        {
            ContractId = Guid.NewGuid(),
            TenantId = tenant1.TenantId,
            EquipmentId = eq2.EquipmentId,
            Status = ContractStatusEnum.Active,
            StartDate = new DateOnly(2026, 3, 1),
            EndDate = new DateOnly(2026, 9, 1),
            BillingModel = BillingModelEnum.FixedPeriod,
            BaseMonthlyRate = 45_000.00m,
            DailyRate = 1_800.00m,
            MaxHoursIncluded = 176,
            OvertimeMultiplier = 1.50m,
            DepositType = "CASH",
            GeofenceId = geofence.GeofenceId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var contract2 = new RentalContract
        {
            ContractId = Guid.NewGuid(),
            TenantId = tenant2.TenantId,
            EquipmentId = eq1.EquipmentId,
            Status = ContractStatusEnum.Draft,
            StartDate = new DateOnly(2026, 5, 1),
            BillingModel = BillingModelEnum.HourlyUsage,
            BaseMonthlyRate = 55_000.00m,
            DailyRate = 2_200.00m,
            MaxHoursIncluded = 200,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        context.RentalContracts.AddRange(contract1, contract2);

        // --- Maintenance Schedules (1 por equipo) ---
        var schedules = new[]
        {
            new MaintenanceSchedule { ScheduleId = Guid.NewGuid(), EquipmentId = eq1.EquipmentId, ServiceTier = "Servicio_250h", IntervalHours = 250, IntervalDays = 180, LastServiceHours = 120.5m, LastServiceDate = new DateOnly(2026, 1, 15), NextDueHours = 370.5m, NextDueDate = new DateOnly(2026, 7, 15), IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new MaintenanceSchedule { ScheduleId = Guid.NewGuid(), EquipmentId = eq2.EquipmentId, ServiceTier = "Servicio_500h", IntervalHours = 500, IntervalDays = 365, LastServiceHours = 890.0m, LastServiceDate = new DateOnly(2025, 12, 1), NextDueHours = 1390.0m, NextDueDate = new DateOnly(2026, 12, 1), IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new MaintenanceSchedule { ScheduleId = Guid.NewGuid(), EquipmentId = eq3.EquipmentId, ServiceTier = "Servicio_200h", IntervalHours = 200, IntervalDays = 90, LastServiceHours = 50.0m, LastServiceDate = new DateOnly(2026, 3, 1), NextDueHours = 250.0m, NextDueDate = new DateOnly(2026, 5, 30), IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new MaintenanceSchedule { ScheduleId = Guid.NewGuid(), EquipmentId = eq4.EquipmentId, ServiceTier = "Servicio_250h", IntervalHours = 250, IntervalDays = 180, LastServiceHours = 0.0m, LastServiceDate = new DateOnly(2026, 4, 1), NextDueHours = 250.0m, NextDueDate = new DateOnly(2026, 10, 1), IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new MaintenanceSchedule { ScheduleId = Guid.NewGuid(), EquipmentId = eq5.EquipmentId, ServiceTier = "Inspeccion_Anual", IntervalHours = 1000, IntervalDays = 365, LastServiceHours = 320.0m, LastServiceDate = new DateOnly(2025, 11, 15), NextDueHours = 1320.0m, NextDueDate = new DateOnly(2026, 11, 15), IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
        };
        context.MaintenanceSchedules.AddRange(schedules);

        // --- Equipment Certification (ANSI A92 para plataforma de tijera) ---
        var cert = new EquipmentCertification
        {
            CertificationId = Guid.NewGuid(),
            EquipmentId = eq2.EquipmentId,
            CertificationName = "ANSI A92.22 Inspeccion Anual",
            CertificationType = "annual_inspection",
            IssuedBy = "Bureau Veritas Mexico",
            IssueDate = new DateOnly(2025, 9, 15),
            ExpirationDate = new DateOnly(2026, 9, 15),
            BlocksRental = true,
            Status = "VALID",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.EquipmentCertifications.Add(cert);

        await context.SaveChangesAsync();
    }
}
