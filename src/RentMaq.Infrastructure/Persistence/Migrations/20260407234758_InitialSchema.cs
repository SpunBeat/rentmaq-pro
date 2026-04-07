using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentMaq.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:alert_type_enum", "fault_code,impact,tilt,overtemp,overpressure,geofence_breach")
                .Annotation("Npgsql:Enum:billing_model_enum", "fixed_period,hourly_usage,tiered,shift_based")
                .Annotation("Npgsql:Enum:contract_status_enum", "draft,reserved,active,suspended_maintenance,returning,inspection,overdue,in_dispute,closed")
                .Annotation("Npgsql:Enum:damage_attribution_enum", "normal_wear,tenant_attributable,environmental,pre_existing,under_dispute")
                .Annotation("Npgsql:Enum:equipment_status_enum", "available,rented,in_maintenance,decommissioned")
                .Annotation("Npgsql:Enum:equipment_type_enum", "miniexcavator,skid_steer,scissor_lift,boom_lift,compactor,generator,pump")
                .Annotation("Npgsql:Enum:maintenance_order_type_enum", "preventive,corrective,inspection")
                .Annotation("Npgsql:Enum:severity_enum", "info,warning,critical,shutdown")
                .Annotation("Npgsql:Enum:work_order_status_enum", "pending,in_progress,loto_applied,completed,cancelled")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "equipment_load_profiles",
                columns: table => new
                {
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    equipment_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    engine_load_factor = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    pto_load_factor = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    maintenance_interval_hours = table.Column<int>(type: "integer", nullable: true),
                    target_utilization_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    last_updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_load_profiles", x => x.profile_id);
                });

            migrationBuilder.CreateTable(
                name: "equipment_thresholds",
                columns: table => new
                {
                    threshold_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_type = table.Column<int>(type: "integer", nullable: false),
                    equipment_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    threshold_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    warning_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    critical_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    shutdown_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    unit_of_measure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_thresholds", x => x.threshold_id);
                });

            migrationBuilder.CreateTable(
                name: "geofences",
                columns: table => new
                {
                    geofence_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    perimeter = table.Column<Geometry>(type: "geography(polygon, 4326)", nullable: false),
                    is_yard = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofences", x => x.geofence_id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    legal_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rfc = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    tax_regime = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    uso_cfdi_default = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false, defaultValue: "G03"),
                    credit_limit = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    credit_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PENDING_EVALUATION"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.tenant_id);
                    table.CheckConstraint("chk_credit_status", "credit_status IN ('PENDING_EVALUATION','APPROVED','SUSPENDED','BLOCKED')");
                });

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    asset_tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: true),
                    equipment_type = table.Column<int>(type: "integer", nullable: false),
                    weight_tons = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    acquisition_cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    acquisition_date = table.Column<DateOnly>(type: "date", nullable: true),
                    current_status = table.Column<int>(type: "integer", nullable: false),
                    aemp_endpoint_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    load_profile_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment", x => x.equipment_id);
                    table.ForeignKey(
                        name: "FK_equipment_equipment_load_profiles_load_profile_id",
                        column: x => x.load_profile_id,
                        principalTable: "equipment_load_profiles",
                        principalColumn: "profile_id");
                });

            migrationBuilder.CreateTable(
                name: "equipment_certifications",
                columns: table => new
                {
                    certification_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certification_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    certification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    issued_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expiration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    blocks_rental = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipment_certifications", x => x.certification_id);
                    table.CheckConstraint("chk_cert_status", "status IN ('VALID','EXPIRED','REVOKED')");
                    table.ForeignKey(
                        name: "FK_equipment_certifications_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                columns: table => new
                {
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    interval_hours = table.Column<int>(type: "integer", nullable: true),
                    interval_days = table.Column<int>(type: "integer", nullable: true),
                    last_service_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    last_service_date = table.Column<DateOnly>(type: "date", nullable: true),
                    next_due_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    next_due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_schedules", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_maintenance_schedules_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rental_contracts",
                columns: table => new
                {
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    billing_model = table.Column<int>(type: "integer", nullable: false),
                    base_monthly_rate = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    daily_rate = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    weekly_rate = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    overtime_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    max_hours_included = table.Column<int>(type: "integer", nullable: true),
                    deposit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    insurance_policy_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rental_contracts", x => x.contract_id);
                    table.ForeignKey(
                        name: "FK_rental_contracts_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rental_contracts_geofences_geofence_id",
                        column: x => x.geofence_id,
                        principalTable: "geofences",
                        principalColumn: "geofence_id");
                    table.ForeignKey(
                        name: "FK_rental_contracts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_alerts",
                columns: table => new
                {
                    alert_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    alert_type = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    spn = table.Column<int>(type: "integer", nullable: true),
                    fmi = table.Column<int>(type: "integer", nullable: true),
                    threshold_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    actual_value = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    impact_g_x = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    impact_g_y = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    impact_g_z = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tilt_lateral_deg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tilt_longitudinal_deg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    attributed_to_tenant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    acknowledged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    acknowledged_by = table.Column<Guid>(type: "uuid", nullable: true),
                    acknowledged_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_alerts", x => x.alert_id);
                    table.ForeignKey(
                        name: "FK_telemetry_alerts_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_readings",
                columns: table => new
                {
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location = table.Column<Point>(type: "geography(point, 4326)", nullable: false),
                    altitude = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    heading = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    speed_kmh = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    hdop = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    satellites = table.Column<int>(type: "integer", nullable: true),
                    engine_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    pto_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    idle_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    cumulative_idle_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    cumulative_idle_non_operating_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    distance = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    fuel_used = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    fuel_level = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    def_used = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    def_level = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    engine_status = table.Column<int>(type: "integer", nullable: true),
                    load_factor = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    active_switches = table.Column<string>(type: "jsonb", nullable: true),
                    impact_g_x = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    impact_g_y = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    impact_g_z = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tilt_lateral = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tilt_longitudinal = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    hydraulic_pressure = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    engine_temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ambient_temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_readings", x => new { x.snapshot_id, x.recorded_at });
                    table.ForeignKey(
                        name: "FK_telemetry_readings_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cfdi_documents",
                columns: table => new
                {
                    cfdi_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cfdi_type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    uuid_fiscal = table.Column<Guid>(type: "uuid", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    uso_cfdi = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    forma_pago = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    related_cfdi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    relation_type = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    cancellation_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    xml_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pdf_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pac_provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfdi_documents", x => x.cfdi_id);
                    table.CheckConstraint("chk_cfdi_status", "status IN ('TIMBRADO','CANCELADO','PENDIENTE_PAGO','PAGADO')");
                    table.CheckConstraint("chk_cfdi_type", "cfdi_type IN ('I','E','P')");
                    table.CheckConstraint("chk_payment_method", "payment_method IN ('PUE','PPD')");
                    table.ForeignKey(
                        name: "FK_cfdi_documents_cfdi_documents_related_cfdi_id",
                        column: x => x.related_cfdi_id,
                        principalTable: "cfdi_documents",
                        principalColumn: "cfdi_id");
                    table.ForeignKey(
                        name: "FK_cfdi_documents_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id");
                });

            migrationBuilder.CreateTable(
                name: "inspection_checklists",
                columns: table => new
                {
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inspection_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inspector_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_items = table.Column<string>(type: "jsonb", nullable: false),
                    photos = table.Column<string>(type: "jsonb", nullable: true),
                    overall_result = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    horometer_reading = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    fuel_level_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    inspected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inspection_checklists", x => x.checklist_id);
                    table.CheckConstraint("chk_inspection_type", "inspection_type IN ('PRE_DELIVERY','POST_RETURN')");
                    table.CheckConstraint("chk_overall_result", "overall_result IN ('APPROVED','APPROVED_WITH_OBSERVATIONS','REJECTED')");
                    table.ForeignKey(
                        name: "FK_inspection_checklists_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inspection_checklists_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id");
                });

            migrationBuilder.CreateTable(
                name: "operator_certifications",
                columns: table => new
                {
                    operator_cert_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operator_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dc3_certificate_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    equipment_type_certified = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_at = table.Column<DateOnly>(type: "date", nullable: true),
                    document_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator_certifications", x => x.operator_cert_id);
                    table.ForeignKey(
                        name: "FK_operator_certifications_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id");
                });

            migrationBuilder.CreateTable(
                name: "maintenance_work_orders",
                columns: table => new
                {
                    work_order_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linked_alert_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    trigger_source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    execution_date = table.Column<DateOnly>(type: "date", nullable: false),
                    loto_applied = table.Column<bool>(type: "boolean", nullable: false),
                    loto_applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    loto_timeout_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    protectors_reinstalled = table.Column<bool>(type: "boolean", nullable: false),
                    protectors_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    technician_notes = table.Column<string>(type: "text", nullable: true),
                    performed_by_worker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parts_used = table.Column<string>(type: "jsonb", nullable: true),
                    labor_hours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    total_cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    next_service_due_at = table.Column<DateOnly>(type: "date", nullable: true),
                    next_service_due_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_work_orders", x => x.work_order_id);
                    table.CheckConstraint("chk_trigger_source", "trigger_source IN ('TELEMETRY_AUTO','CALENDAR_AUTO','MANUAL_INSPECTION','TENANT_REPORT')");
                    table.ForeignKey(
                        name: "FK_maintenance_work_orders_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_work_orders_maintenance_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "maintenance_schedules",
                        principalColumn: "schedule_id");
                    table.ForeignKey(
                        name: "FK_maintenance_work_orders_telemetry_alerts_linked_alert_id",
                        column: x => x.linked_alert_id,
                        principalTable: "telemetry_alerts",
                        principalColumn: "alert_id");
                });

            migrationBuilder.CreateTable(
                name: "deposits",
                columns: table => new
                {
                    deposit_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    applied_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    refunded_amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    related_cfdi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    accounting_classification = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "LIABILITY"),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposits", x => x.deposit_id);
                    table.CheckConstraint("chk_accounting_class", "accounting_classification IN ('LIABILITY','RECOGNIZED_INCOME')");
                    table.CheckConstraint("chk_deposit_balance", "applied_amount + refunded_amount <= amount");
                    table.CheckConstraint("chk_deposit_status", "status IN ('PENDING_COLLECTION','HELD_AS_LIABILITY','PARTIALLY_APPLIED','FULLY_APPLIED','REFUNDED')");
                    table.ForeignKey(
                        name: "FK_deposits_cfdi_documents_related_cfdi_id",
                        column: x => x.related_cfdi_id,
                        principalTable: "cfdi_documents",
                        principalColumn: "cfdi_id");
                    table.ForeignKey(
                        name: "FK_deposits_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "damage_assessments",
                columns: table => new
                {
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fault_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: true),
                    work_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inspection_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    assessor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    damage_description = table.Column<string>(type: "text", nullable: false),
                    attribution = table.Column<int>(type: "integer", nullable: false),
                    estimated_repair_cost = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    customer_signature_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    photos = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_assessments", x => x.assessment_id);
                    table.CheckConstraint("chk_damage_status", "status IN ('DRAFT','UNDER_REVIEW','APPROVED_FOR_CHARGE','DISMISSED')");
                    table.ForeignKey(
                        name: "FK_damage_assessments_equipment_equipment_id",
                        column: x => x.equipment_id,
                        principalTable: "equipment",
                        principalColumn: "equipment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_damage_assessments_inspection_checklists_checklist_id",
                        column: x => x.checklist_id,
                        principalTable: "inspection_checklists",
                        principalColumn: "checklist_id");
                    table.ForeignKey(
                        name: "FK_damage_assessments_maintenance_work_orders_work_order_id",
                        column: x => x.work_order_id,
                        principalTable: "maintenance_work_orders",
                        principalColumn: "work_order_id");
                    table.ForeignKey(
                        name: "FK_damage_assessments_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_damage_assessments_telemetry_alerts_fault_event_id",
                        column: x => x.fault_event_id,
                        principalTable: "telemetry_alerts",
                        principalColumn: "alert_id");
                });

            migrationBuilder.CreateTable(
                name: "extraordinary_charges",
                columns: table => new
                {
                    charge_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deposit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cfdi_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false),
                    amount_from_deposit = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    amount_direct_bill = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: false, defaultValue: 0m),
                    reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraordinary_charges", x => x.charge_id);
                    table.CheckConstraint("chk_charge_status", "status IN ('DETECTED','ATTRIBUTED','APPLIED_TO_DEPOSIT','INVOICED')");
                    table.ForeignKey(
                        name: "FK_extraordinary_charges_cfdi_documents_cfdi_id",
                        column: x => x.cfdi_id,
                        principalTable: "cfdi_documents",
                        principalColumn: "cfdi_id");
                    table.ForeignKey(
                        name: "FK_extraordinary_charges_damage_assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "damage_assessments",
                        principalColumn: "assessment_id");
                    table.ForeignKey(
                        name: "FK_extraordinary_charges_deposits_deposit_id",
                        column: x => x.deposit_id,
                        principalTable: "deposits",
                        principalColumn: "deposit_id");
                    table.ForeignKey(
                        name: "FK_extraordinary_charges_rental_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "rental_contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cfdi_documents_contract_id",
                table: "cfdi_documents",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_cfdi_documents_related_cfdi_id",
                table: "cfdi_documents",
                column: "related_cfdi_id");

            migrationBuilder.CreateIndex(
                name: "IX_cfdi_documents_uuid_fiscal",
                table: "cfdi_documents",
                column: "uuid_fiscal",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_damage_assessments_checklist_id",
                table: "damage_assessments",
                column: "checklist_id");

            migrationBuilder.CreateIndex(
                name: "IX_damage_assessments_contract_id",
                table: "damage_assessments",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_damage_assessments_equipment_id",
                table: "damage_assessments",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_damage_assessments_fault_event_id",
                table: "damage_assessments",
                column: "fault_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_damage_assessments_work_order_id",
                table: "damage_assessments",
                column: "work_order_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposits_contract_id",
                table: "deposits",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposits_related_cfdi_id",
                table: "deposits",
                column: "related_cfdi_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_asset_tag",
                table: "equipment",
                column: "asset_tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipment_load_profile_id",
                table: "equipment",
                column: "load_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_serial_number",
                table: "equipment",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipment_certifications_equipment_id",
                table: "equipment_certifications",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipment_load_profiles_equipment_make_equipment_model_appl~",
                table: "equipment_load_profiles",
                columns: new[] { "equipment_make", "equipment_model", "application_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipment_thresholds_equipment_type_equipment_model_thresho~",
                table: "equipment_thresholds",
                columns: new[] { "equipment_type", "equipment_model", "threshold_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_extraordinary_charges_assessment_id",
                table: "extraordinary_charges",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraordinary_charges_cfdi_id",
                table: "extraordinary_charges",
                column: "cfdi_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraordinary_charges_contract_id",
                table: "extraordinary_charges",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraordinary_charges_deposit_id",
                table: "extraordinary_charges",
                column: "deposit_id");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_checklists_contract_id",
                table: "inspection_checklists",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_inspection_checklists_equipment_id",
                table: "inspection_checklists",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_schedules_equipment_id",
                table: "maintenance_schedules",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_work_orders_equipment_id_execution_date",
                table: "maintenance_work_orders",
                columns: new[] { "equipment_id", "execution_date" });

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_work_orders_linked_alert_id",
                table: "maintenance_work_orders",
                column: "linked_alert_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_work_orders_schedule_id",
                table: "maintenance_work_orders",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_work_orders_status",
                table: "maintenance_work_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_operator_certifications_contract_id",
                table: "operator_certifications",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_contracts_equipment_id",
                table: "rental_contracts",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_contracts_geofence_id",
                table: "rental_contracts",
                column: "geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_contracts_tenant_id",
                table: "rental_contracts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_alerts_detected_at",
                table: "telemetry_alerts",
                column: "detected_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_alerts_equipment_id_severity_resolved",
                table: "telemetry_alerts",
                columns: new[] { "equipment_id", "severity", "resolved" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_readings_equipment_id",
                table: "telemetry_readings",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_rfc",
                table: "tenants",
                column: "rfc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_certifications");

            migrationBuilder.DropTable(
                name: "equipment_thresholds");

            migrationBuilder.DropTable(
                name: "extraordinary_charges");

            migrationBuilder.DropTable(
                name: "operator_certifications");

            migrationBuilder.DropTable(
                name: "telemetry_readings");

            migrationBuilder.DropTable(
                name: "damage_assessments");

            migrationBuilder.DropTable(
                name: "deposits");

            migrationBuilder.DropTable(
                name: "inspection_checklists");

            migrationBuilder.DropTable(
                name: "maintenance_work_orders");

            migrationBuilder.DropTable(
                name: "cfdi_documents");

            migrationBuilder.DropTable(
                name: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "telemetry_alerts");

            migrationBuilder.DropTable(
                name: "rental_contracts");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "geofences");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "equipment_load_profiles");
        }
    }
}
