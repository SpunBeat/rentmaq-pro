-- =============================================================================
-- RentMaq Pro — Script de inicializacion de base de datos
-- Ejecutado automaticamente por PostgreSQL al crear la BD por primera vez
-- (montado en /docker-entrypoint-initdb.d/).
--
-- Contenido:
--   1. Extensiones requeridas (PostGIS, uuid-ossp)
--   2. Los 9 ENUMs definidos en ADR-001 Seccion 3.1
--
-- Este script es idempotente: usa IF NOT EXISTS en todas las sentencias.
-- =============================================================================

-- =============================================================================
-- EXTENSIONES
-- =============================================================================

-- PostGIS: requerido para GEOGRAPHY(POLYGON, 4326), GEOGRAPHY(POINT, 4326),
-- ST_Covers y calculo de distancias reales sobre curvatura terrestre.
CREATE EXTENSION IF NOT EXISTS postgis;

-- uuid-ossp: funcion uuid_generate_v4() como alternativa a gen_random_uuid().
-- gen_random_uuid() esta disponible nativamente en PG 13+, pero uuid-ossp
-- se mantiene por compatibilidad con herramientas externas.
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================================================
-- ENUMS (ADR-001 Seccion 3.1)
-- =============================================================================

-- Clasificacion por tipo de maquinaria
DO $$ BEGIN
    CREATE TYPE equipment_type_enum AS ENUM (
        'MINIEXCAVATOR', 'SKID_STEER', 'SCISSOR_LIFT',
        'BOOM_LIFT', 'COMPACTOR', 'GENERATOR', 'PUMP'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- State machine del activo fisico (operada por Worker 3)
DO $$ BEGIN
    CREATE TYPE equipment_status_enum AS ENUM (
        'AVAILABLE', 'RENTED', 'IN_MAINTENANCE', 'DECOMMISSIONED'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Modelo de facturacion del contrato
DO $$ BEGIN
    CREATE TYPE billing_model_enum AS ENUM (
        'FIXED_PERIOD', 'HOURLY_USAGE', 'TIERED', 'SHIFT_BASED'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- State machine del contrato (9 estados, operada por Worker 7)
DO $$ BEGIN
    CREATE TYPE contract_status_enum AS ENUM (
        'DRAFT', 'RESERVED', 'ACTIVE', 'SUSPENDED_MAINTENANCE',
        'RETURNING', 'INSPECTION', 'OVERDUE', 'IN_DISPUTE', 'CLOSED'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Tipo de alerta telematica
DO $$ BEGIN
    CREATE TYPE alert_type_enum AS ENUM (
        'FAULT_CODE', 'IMPACT', 'TILT', 'OVERTEMP',
        'OVERPRESSURE', 'GEOFENCE_BREACH'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Severidad de alerta
DO $$ BEGIN
    CREATE TYPE severity_enum AS ENUM (
        'INFO', 'WARNING', 'CRITICAL', 'SHUTDOWN'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Atribucion de dano (reemplaza booleano is_chargeable)
DO $$ BEGIN
    CREATE TYPE damage_attribution_enum AS ENUM (
        'NORMAL_WEAR', 'TENANT_ATTRIBUTABLE', 'ENVIRONMENTAL',
        'PRE_EXISTING', 'UNDER_DISPUTE'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Tipo de orden de mantenimiento
DO $$ BEGIN
    CREATE TYPE maintenance_order_type_enum AS ENUM (
        'PREVENTIVE', 'CORRECTIVE', 'INSPECTION'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Estado de orden de trabajo
DO $$ BEGIN
    CREATE TYPE work_order_status_enum AS ENUM (
        'PENDING', 'IN_PROGRESS', 'LOTO_APPLIED', 'COMPLETED', 'CANCELLED'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- =============================================================================
-- Verificacion
-- =============================================================================
DO $$
BEGIN
    RAISE NOTICE '=== RentMaq Pro: init-db.sql completado ===';
    RAISE NOTICE 'PostGIS version: %', PostGIS_Version();
    RAISE NOTICE 'ENUMs creados: 9 tipos (equipment_type, equipment_status, billing_model, contract_status, alert_type, severity, damage_attribution, maintenance_order_type, work_order_status)';
END $$;
