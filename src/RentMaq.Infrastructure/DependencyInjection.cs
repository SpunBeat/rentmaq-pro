using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentMaq.Domain.Interfaces;
using RentMaq.Infrastructure.Persistence;
using RentMaq.Infrastructure.Repositories;

namespace RentMaq.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RentMaqDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("RentMaqDb"),
                npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Existing repositories
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IRentalContractRepository, RentalContractRepository>();
        services.AddScoped<ITelemetryRepository, TelemetryRepository>();
        services.AddScoped<IMaintenanceWorkOrderRepository, MaintenanceWorkOrderRepository>();

        // New repositories
        services.AddScoped<IDamageAssessmentRepository, DamageAssessmentRepository>();
        services.AddScoped<ICfdiDocumentRepository, CfdiDocumentRepository>();
        services.AddScoped<IDepositRepository, DepositRepository>();
        services.AddScoped<IExtraordinaryChargeRepository, ExtraordinaryChargeRepository>();
        services.AddScoped<IGeofenceRepository, GeofenceRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMaintenanceScheduleRepository, MaintenanceScheduleRepository>();
        services.AddScoped<IEquipmentCertificationRepository, EquipmentCertificationRepository>();
        services.AddScoped<IInspectionChecklistRepository, InspectionChecklistRepository>();

        return services;
    }
}
