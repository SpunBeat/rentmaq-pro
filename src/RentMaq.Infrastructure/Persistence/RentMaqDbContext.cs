using Microsoft.EntityFrameworkCore;
using RentMaq.Domain.Entities;

namespace RentMaq.Infrastructure.Persistence;

public class RentMaqDbContext : DbContext
{
    public RentMaqDbContext(DbContextOptions<RentMaqDbContext> options) : base(options) { }

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<EquipmentLoadProfile> EquipmentLoadProfiles => Set<EquipmentLoadProfile>();
    public DbSet<EquipmentThreshold> EquipmentThresholds => Set<EquipmentThreshold>();
    public DbSet<Geofence> Geofences => Set<Geofence>();
    public DbSet<TelemetryReading> TelemetryReadings => Set<TelemetryReading>();
    public DbSet<TelemetryAlert> TelemetryAlerts => Set<TelemetryAlert>();
    public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();
    public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders => Set<MaintenanceWorkOrder>();
    public DbSet<EquipmentCertification> EquipmentCertifications => Set<EquipmentCertification>();
    public DbSet<OperatorCertification> OperatorCertifications => Set<OperatorCertification>();
    public DbSet<InspectionChecklist> InspectionChecklists => Set<InspectionChecklist>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RentalContract> RentalContracts => Set<RentalContract>();
    public DbSet<CfdiDocument> CfdiDocuments => Set<CfdiDocument>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<DamageAssessment> DamageAssessments => Set<DamageAssessment>();
    public DbSet<ExtraordinaryCharge> ExtraordinaryCharges => Set<ExtraordinaryCharge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map all enums to PostgreSQL string enums via Npgsql
        modelBuilder.HasPostgresEnum<Domain.Enums.EquipmentTypeEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.EquipmentStatusEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.BillingModelEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.ContractStatusEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.AlertTypeEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.SeverityEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.DamageAttributionEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.MaintenanceOrderTypeEnum>();
        modelBuilder.HasPostgresEnum<Domain.Enums.WorkOrderStatusEnum>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentMaqDbContext).Assembly);
    }
}
