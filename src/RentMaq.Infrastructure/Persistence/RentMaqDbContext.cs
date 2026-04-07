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

        // Equipment
        modelBuilder.Entity<Equipment>(e =>
        {
            e.ToTable("equipment");
            e.HasKey(x => x.EquipmentId);
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.AssetTag).HasColumnName("asset_tag").HasMaxLength(50);
            e.Property(x => x.SerialNumber).HasColumnName("serial_number").HasMaxLength(100);
            e.Property(x => x.Make).HasColumnName("make").HasMaxLength(50);
            e.Property(x => x.Model).HasColumnName("model").HasMaxLength(50);
            e.Property(x => x.Year).HasColumnName("year");
            e.Property(x => x.EquipmentType).HasColumnName("equipment_type");
            e.Property(x => x.WeightTons).HasColumnName("weight_tons");
            e.Property(x => x.AcquisitionCost).HasColumnName("acquisition_cost");
            e.Property(x => x.AcquisitionDate).HasColumnName("acquisition_date");
            e.Property(x => x.CurrentStatus).HasColumnName("current_status");
            e.Property(x => x.AempEndpointUrl).HasColumnName("aemp_endpoint_url").HasMaxLength(500);
            e.Property(x => x.LoadProfileId).HasColumnName("load_profile_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.AssetTag).IsUnique();
            e.HasIndex(x => x.SerialNumber).IsUnique();
        });

        // EquipmentLoadProfile
        modelBuilder.Entity<EquipmentLoadProfile>(e =>
        {
            e.ToTable("equipment_load_profiles");
            e.HasKey(x => x.ProfileId);
            e.Property(x => x.ProfileId).HasColumnName("profile_id");
            e.Property(x => x.EquipmentMake).HasColumnName("equipment_make").HasMaxLength(50);
            e.Property(x => x.EquipmentModel).HasColumnName("equipment_model").HasMaxLength(50);
            e.Property(x => x.ApplicationType).HasColumnName("application_type").HasMaxLength(50);
            e.Property(x => x.EngineLoadFactor).HasColumnName("engine_load_factor");
            e.Property(x => x.PtoLoadFactor).HasColumnName("pto_load_factor");
            e.Property(x => x.MaintenanceIntervalHours).HasColumnName("maintenance_interval_hours");
            e.Property(x => x.TargetUtilizationPct).HasColumnName("target_utilization_pct");
            e.Property(x => x.LastUpdated).HasColumnName("last_updated");
            e.HasIndex(x => new { x.EquipmentMake, x.EquipmentModel, x.ApplicationType }).IsUnique();
        });

        // EquipmentThreshold
        modelBuilder.Entity<EquipmentThreshold>(e =>
        {
            e.ToTable("equipment_thresholds");
            e.HasKey(x => x.ThresholdId);
            e.Property(x => x.ThresholdId).HasColumnName("threshold_id");
            e.Property(x => x.EquipmentType).HasColumnName("equipment_type");
            e.Property(x => x.EquipmentModel).HasColumnName("equipment_model").HasMaxLength(50);
            e.Property(x => x.ThresholdType).HasColumnName("threshold_type").HasMaxLength(30);
            e.Property(x => x.WarningValue).HasColumnName("warning_value");
            e.Property(x => x.CriticalValue).HasColumnName("critical_value");
            e.Property(x => x.ShutdownValue).HasColumnName("shutdown_value");
            e.Property(x => x.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(20);
            e.HasIndex(x => new { x.EquipmentType, x.EquipmentModel, x.ThresholdType }).IsUnique();
        });

        // Geofence
        modelBuilder.Entity<Geofence>(e =>
        {
            e.ToTable("geofences");
            e.HasKey(x => x.GeofenceId);
            e.Property(x => x.GeofenceId).HasColumnName("geofence_id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            e.Property(x => x.Perimeter).HasColumnName("perimeter").HasColumnType("geography(polygon, 4326)");
            e.Property(x => x.IsYard).HasColumnName("is_yard");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // TelemetryReading (composite PK)
        modelBuilder.Entity<TelemetryReading>(e =>
        {
            e.ToTable("telemetry_readings");
            e.HasKey(x => new { x.SnapshotId, x.RecordedAt });
            e.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.RecordedAt).HasColumnName("recorded_at");
            e.Property(x => x.Location).HasColumnName("location").HasColumnType("geography(point, 4326)");
            e.Property(x => x.Altitude).HasColumnName("altitude");
            e.Property(x => x.Heading).HasColumnName("heading");
            e.Property(x => x.SpeedKmh).HasColumnName("speed_kmh");
            e.Property(x => x.Hdop).HasColumnName("hdop");
            e.Property(x => x.Satellites).HasColumnName("satellites");
            e.Property(x => x.EngineHours).HasColumnName("engine_hours");
            e.Property(x => x.PtoHours).HasColumnName("pto_hours");
            e.Property(x => x.IdleHours).HasColumnName("idle_hours");
            e.Property(x => x.CumulativeIdleHours).HasColumnName("cumulative_idle_hours");
            e.Property(x => x.CumulativeIdleNonOperatingHours).HasColumnName("cumulative_idle_non_operating_hours");
            e.Property(x => x.Distance).HasColumnName("distance");
            e.Property(x => x.FuelUsed).HasColumnName("fuel_used");
            e.Property(x => x.FuelLevel).HasColumnName("fuel_level");
            e.Property(x => x.DefUsed).HasColumnName("def_used");
            e.Property(x => x.DefLevel).HasColumnName("def_level");
            e.Property(x => x.EngineStatus).HasColumnName("engine_status");
            e.Property(x => x.LoadFactor).HasColumnName("load_factor");
            e.Property(x => x.ActiveSwitches).HasColumnName("active_switches").HasColumnType("jsonb");
            e.Property(x => x.ImpactGX).HasColumnName("impact_g_x");
            e.Property(x => x.ImpactGY).HasColumnName("impact_g_y");
            e.Property(x => x.ImpactGZ).HasColumnName("impact_g_z");
            e.Property(x => x.TiltLateral).HasColumnName("tilt_lateral");
            e.Property(x => x.TiltLongitudinal).HasColumnName("tilt_longitudinal");
            e.Property(x => x.HydraulicPressure).HasColumnName("hydraulic_pressure");
            e.Property(x => x.EngineTemperature).HasColumnName("engine_temperature");
            e.Property(x => x.AmbientTemperature).HasColumnName("ambient_temperature");
        });

        // TelemetryAlert
        modelBuilder.Entity<TelemetryAlert>(e =>
        {
            e.ToTable("telemetry_alerts");
            e.HasKey(x => x.AlertId);
            e.Property(x => x.AlertId).HasColumnName("alert_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.SnapshotId).HasColumnName("snapshot_id");
            e.Property(x => x.DetectedAt).HasColumnName("detected_at");
            e.Property(x => x.AlertType).HasColumnName("alert_type");
            e.Property(x => x.Severity).HasColumnName("severity");
            e.Property(x => x.Spn).HasColumnName("spn");
            e.Property(x => x.Fmi).HasColumnName("fmi");
            e.Property(x => x.ThresholdValue).HasColumnName("threshold_value");
            e.Property(x => x.ActualValue).HasColumnName("actual_value");
            e.Property(x => x.ImpactGX).HasColumnName("impact_g_x");
            e.Property(x => x.ImpactGY).HasColumnName("impact_g_y");
            e.Property(x => x.ImpactGZ).HasColumnName("impact_g_z");
            e.Property(x => x.TiltLateralDeg).HasColumnName("tilt_lateral_deg");
            e.Property(x => x.TiltLongitudinalDeg).HasColumnName("tilt_longitudinal_deg");
            e.Property(x => x.AttributedToTenant).HasColumnName("attributed_to_tenant");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Acknowledged).HasColumnName("acknowledged");
            e.Property(x => x.AcknowledgedBy).HasColumnName("acknowledged_by");
            e.Property(x => x.AcknowledgedAt).HasColumnName("acknowledged_at");
            e.Property(x => x.Resolved).HasColumnName("resolved");
            e.HasIndex(x => new { x.EquipmentId, x.Severity, x.Resolved });
            e.HasIndex(x => x.DetectedAt).IsDescending();
        });

        // MaintenanceSchedule
        modelBuilder.Entity<MaintenanceSchedule>(e =>
        {
            e.ToTable("maintenance_schedules");
            e.HasKey(x => x.ScheduleId);
            e.Property(x => x.ScheduleId).HasColumnName("schedule_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.ServiceTier).HasColumnName("service_tier").HasMaxLength(50);
            e.Property(x => x.IntervalHours).HasColumnName("interval_hours");
            e.Property(x => x.IntervalDays).HasColumnName("interval_days");
            e.Property(x => x.LastServiceHours).HasColumnName("last_service_hours");
            e.Property(x => x.LastServiceDate).HasColumnName("last_service_date");
            e.Property(x => x.NextDueHours).HasColumnName("next_due_hours");
            e.Property(x => x.NextDueDate).HasColumnName("next_due_date");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // MaintenanceWorkOrder
        modelBuilder.Entity<MaintenanceWorkOrder>(e =>
        {
            e.ToTable("maintenance_work_orders");
            e.HasKey(x => x.WorkOrderId);
            e.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.ScheduleId).HasColumnName("schedule_id");
            e.Property(x => x.LinkedAlertId).HasColumnName("linked_alert_id");
            e.Property(x => x.OrderType).HasColumnName("order_type");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.TriggerSource).HasColumnName("trigger_source").HasMaxLength(30);
            e.Property(x => x.ExecutionDate).HasColumnName("execution_date");
            e.Property(x => x.LotoApplied).HasColumnName("loto_applied");
            e.Property(x => x.LotoAppliedAt).HasColumnName("loto_applied_at");
            e.Property(x => x.LotoTimeoutHours).HasColumnName("loto_timeout_hours");
            e.Property(x => x.ProtectorsReinstalled).HasColumnName("protectors_reinstalled");
            e.Property(x => x.ProtectorsVerifiedAt).HasColumnName("protectors_verified_at");
            e.Property(x => x.TechnicianNotes).HasColumnName("technician_notes");
            e.Property(x => x.PerformedByWorkerId).HasColumnName("performed_by_worker_id");
            e.Property(x => x.PartsUsed).HasColumnName("parts_used").HasColumnType("jsonb");
            e.Property(x => x.LaborHours).HasColumnName("labor_hours");
            e.Property(x => x.TotalCost).HasColumnName("total_cost");
            e.Property(x => x.NextServiceDueAt).HasColumnName("next_service_due_at");
            e.Property(x => x.NextServiceDueHours).HasColumnName("next_service_due_hours");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => new { x.EquipmentId, x.ExecutionDate });
            e.HasIndex(x => x.Status);
        });

        // EquipmentCertification
        modelBuilder.Entity<EquipmentCertification>(e =>
        {
            e.ToTable("equipment_certifications");
            e.HasKey(x => x.CertificationId);
            e.Property(x => x.CertificationId).HasColumnName("certification_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.CertificationName).HasColumnName("certification_name").HasMaxLength(100);
            e.Property(x => x.CertificationType).HasColumnName("certification_type").HasMaxLength(50);
            e.Property(x => x.IssuedBy).HasColumnName("issued_by").HasMaxLength(200);
            e.Property(x => x.IssueDate).HasColumnName("issue_date");
            e.Property(x => x.ExpirationDate).HasColumnName("expiration_date");
            e.Property(x => x.BlocksRental).HasColumnName("blocks_rental");
            e.Property(x => x.DocumentUrl).HasColumnName("document_url").HasMaxLength(500);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.ExpirationDate).HasFilter("status = 'VALID'");
        });

        // OperatorCertification
        modelBuilder.Entity<OperatorCertification>(e =>
        {
            e.ToTable("operator_certifications");
            e.HasKey(x => x.OperatorCertId);
            e.Property(x => x.OperatorCertId).HasColumnName("operator_cert_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(200);
            e.Property(x => x.Dc3CertificateNumber).HasColumnName("dc3_certificate_number").HasMaxLength(100);
            e.Property(x => x.EquipmentTypeCertified).HasColumnName("equipment_type_certified");
            e.Property(x => x.IssuedAt).HasColumnName("issued_at");
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.DocumentUrl).HasColumnName("document_url").HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // InspectionChecklist
        modelBuilder.Entity<InspectionChecklist>(e =>
        {
            e.ToTable("inspection_checklists");
            e.HasKey(x => x.ChecklistId);
            e.Property(x => x.ChecklistId).HasColumnName("checklist_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.InspectionType).HasColumnName("inspection_type").HasMaxLength(20);
            e.Property(x => x.InspectorId).HasColumnName("inspector_id");
            e.Property(x => x.ChecklistItems).HasColumnName("checklist_items").HasColumnType("jsonb");
            e.Property(x => x.Photos).HasColumnName("photos").HasColumnType("jsonb");
            e.Property(x => x.OverallResult).HasColumnName("overall_result").HasMaxLength(30);
            e.Property(x => x.HorometerReading).HasColumnName("horometer_reading");
            e.Property(x => x.FuelLevelPct).HasColumnName("fuel_level_pct");
            e.Property(x => x.InspectedAt).HasColumnName("inspected_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        // Tenant
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(x => x.TenantId);
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.LegalName).HasColumnName("legal_name").HasMaxLength(255);
            e.Property(x => x.Rfc).HasColumnName("rfc").HasMaxLength(13);
            e.Property(x => x.TaxRegime).HasColumnName("tax_regime").HasMaxLength(3);
            e.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(5);
            e.Property(x => x.UsoCfdiDefault).HasColumnName("uso_cfdi_default").HasMaxLength(4);
            e.Property(x => x.CreditLimit).HasColumnName("credit_limit");
            e.Property(x => x.CreditStatus).HasColumnName("credit_status").HasMaxLength(30);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.Rfc).IsUnique();
        });

        // RentalContract
        modelBuilder.Entity<RentalContract>(e =>
        {
            e.ToTable("rental_contracts");
            e.HasKey(x => x.ContractId);
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.TenantId).HasColumnName("tenant_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.StartDate).HasColumnName("start_date");
            e.Property(x => x.EndDate).HasColumnName("end_date");
            e.Property(x => x.BillingModel).HasColumnName("billing_model");
            e.Property(x => x.BaseMonthlyRate).HasColumnName("base_monthly_rate");
            e.Property(x => x.DailyRate).HasColumnName("daily_rate");
            e.Property(x => x.WeeklyRate).HasColumnName("weekly_rate");
            e.Property(x => x.OvertimeMultiplier).HasColumnName("overtime_multiplier");
            e.Property(x => x.MaxHoursIncluded).HasColumnName("max_hours_included");
            e.Property(x => x.DepositType).HasColumnName("deposit_type").HasMaxLength(50);
            e.Property(x => x.InsurancePolicyNumber).HasColumnName("insurance_policy_number").HasMaxLength(100);
            e.Property(x => x.GeofenceId).HasColumnName("geofence_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // CfdiDocument
        modelBuilder.Entity<CfdiDocument>(e =>
        {
            e.ToTable("cfdi_documents");
            e.HasKey(x => x.CfdiId);
            e.Property(x => x.CfdiId).HasColumnName("cfdi_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.CfdiType).HasColumnName("cfdi_type").HasMaxLength(1);
            e.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(3);
            e.Property(x => x.UuidFiscal).HasColumnName("uuid_fiscal");
            e.Property(x => x.TotalAmount).HasColumnName("total_amount");
            e.Property(x => x.UsoCfdi).HasColumnName("uso_cfdi").HasMaxLength(4);
            e.Property(x => x.FormaPago).HasColumnName("forma_pago").HasMaxLength(2);
            e.Property(x => x.RelatedCfdiId).HasColumnName("related_cfdi_id");
            e.Property(x => x.RelationType).HasColumnName("relation_type").HasMaxLength(2);
            e.Property(x => x.CancelReason).HasColumnName("cancel_reason").HasMaxLength(2);
            e.Property(x => x.CancellationStatus).HasColumnName("cancellation_status").HasMaxLength(50);
            e.Property(x => x.XmlUrl).HasColumnName("xml_url").HasMaxLength(500);
            e.Property(x => x.PdfUrl).HasColumnName("pdf_url").HasMaxLength(500);
            e.Property(x => x.PacProvider).HasColumnName("pac_provider").HasMaxLength(100);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(x => x.IssuedAt).HasColumnName("issued_at");
            e.HasIndex(x => x.UuidFiscal).IsUnique();
        });

        // Deposit
        modelBuilder.Entity<Deposit>(e =>
        {
            e.ToTable("deposits");
            e.HasKey(x => x.DepositId);
            e.Property(x => x.DepositId).HasColumnName("deposit_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.Property(x => x.AppliedAmount).HasColumnName("applied_amount");
            e.Property(x => x.RefundedAmount).HasColumnName("refunded_amount");
            e.Property(x => x.RelatedCfdiId).HasColumnName("related_cfdi_id");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.AccountingClassification).HasColumnName("accounting_classification").HasMaxLength(20);
            e.Property(x => x.ReceivedAt).HasColumnName("received_at");
        });

        // DamageAssessment
        modelBuilder.Entity<DamageAssessment>(e =>
        {
            e.ToTable("damage_assessments");
            e.HasKey(x => x.AssessmentId);
            e.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.EquipmentId).HasColumnName("equipment_id");
            e.Property(x => x.FaultEventId).HasColumnName("fault_event_id");
            e.Property(x => x.ChecklistId).HasColumnName("checklist_id");
            e.Property(x => x.WorkOrderId).HasColumnName("work_order_id");
            e.Property(x => x.InspectionDate).HasColumnName("inspection_date");
            e.Property(x => x.AssessorId).HasColumnName("assessor_id");
            e.Property(x => x.DamageDescription).HasColumnName("damage_description");
            e.Property(x => x.Attribution).HasColumnName("attribution");
            e.Property(x => x.EstimatedRepairCost).HasColumnName("estimated_repair_cost");
            e.Property(x => x.CustomerSignatureUrl).HasColumnName("customer_signature_url").HasMaxLength(500);
            e.Property(x => x.Photos).HasColumnName("photos").HasColumnType("jsonb");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        // ExtraordinaryCharge
        modelBuilder.Entity<ExtraordinaryCharge>(e =>
        {
            e.ToTable("extraordinary_charges");
            e.HasKey(x => x.ChargeId);
            e.Property(x => x.ChargeId).HasColumnName("charge_id");
            e.Property(x => x.ContractId).HasColumnName("contract_id");
            e.Property(x => x.DepositId).HasColumnName("deposit_id");
            e.Property(x => x.AssessmentId).HasColumnName("assessment_id");
            e.Property(x => x.CfdiId).HasColumnName("cfdi_id");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.Amount).HasColumnName("amount");
            e.Property(x => x.AmountFromDeposit).HasColumnName("amount_from_deposit");
            e.Property(x => x.AmountDirectBill).HasColumnName("amount_direct_bill");
            e.Property(x => x.Reason).HasColumnName("reason");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });
    }
}
