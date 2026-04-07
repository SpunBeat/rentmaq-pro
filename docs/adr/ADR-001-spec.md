# ADR-001: Esquema de Base de Datos y Workers de Orquestacion para Telemetria, Mantenimiento y Facturacion

## 1. Metadata

| Campo       | Valor |
|-------------|-------|
| **ID**      | ADR-001 |
| **Estado**  | Propuesto |
| **Fecha**   | 2026-04-06 |
| **Autor**   | Noe (Director de Arquitectura) |
| **Stack**   | .NET Core 8, PostgreSQL + PostGIS, Angular Material |
| **Dev Tooling** | Google Conductor (Gemini CLI), Google Antigravity |
| **Normativas** | ISO 15143-3 (AEMP 2.0), J1939, NOM-004-STPS-1999, NOM-006-STPS-2014, NOM-009-STPS-2011, CFDI 4.0, NIF D-5, Resolucion 42/IVA/2019-RF |

---

## 2. Contexto y Problema

El modelo operativo de arrendamiento de maquinaria (RentMaq Pro) converge en la interseccion de tres dominios altamente complejos y estrictamente regulados: la ingenieria de datos y telemetria (estandares ISO 15143-3 y protocolos J1939), el marco fiscal y transaccional financiero (normativas del SAT para CFDI 4.0, NIF D-5 y gestion de pasivos), y el cumplimiento normativo laboral y de seguridad industrial (NOM-004-STPS-1999, NOM-006-STPS-2014 y NOM-009-STPS-2011 para plataformas de elevacion).

### La interdependencia critica

Existe una codependencia absoluta entre los flujos tecnicos, financieros y legales, donde un unico evento fisico en el equipo de renta desencadena una reaccion en cadena en todo el ecosistema del ERP. Por ejemplo, si los sensores de telemetria detectan un evento de impacto severo (medido en +/-g por eje) o si el inclinometro supera los umbrales de seguridad configurados para ese tipo y modelo de equipo (ej. 2 grados lateral para plataformas de elevacion, con perfiles diferenciados para compactadores cuya operacion normal genera altas vibraciones), la arquitectura del sistema debe traducir inmediatamente este dato tecnico en dos dimensiones operativas:

**Cumplimiento Legal e Integridad Fisica:** El evento critico (CRITICAL o SHUTDOWN) debe disparar de forma automatica una orden de mantenimiento correctivo, forzando la aplicacion del protocolo de aseguramiento de bloqueo de energia (Lockout/Tagout) y la colocacion de candados y tarjetas de aviso, conforme a las exigencias de la STPS. El sistema debe generar registros auditables de la intervencion y de la reinstalacion de protectores, los cuales deben conservarse al menos durante 12 meses (NOM-004 Art. 7.2.3). Para plataformas de elevacion, el sistema debe validar adicionalmente la vigencia de la certificacion anual por tercero autorizado conforme a ANSI A92 (referenciada en NOM-009-STPS-2011).

**Impacto Financiero y Fiscal:** Simultaneamente, al confirmarse mediante telemetria que la falla es atribuible a negligencia del cliente (`attributed_to_tenant`), la maquina de estados del modulo de arrendamiento debe accionar el cobro por mal uso. Esto implica extraer el monto proporcional del Deposito en Garantia (contabilizado como Pasivo sin causar IVA ni ISR conforme a la Resolucion 42/IVA/2019-RF) y trasladarlo a una cuenta de Ingreso gravable. El ERP emite un CFDI de Ingreso nuevo e independiente por el concepto de penalizacion — nunca sustituye ni cancela la factura mensual de renta original, ya que esta fue emitida correctamente. La TipoRelacion 04 (sustitucion) queda reservada exclusivamente para el flujo de correccion de errores en CFDI previamente emitidos, y la TipoRelacion 01 (nota de credito) aplica unicamente cuando el arrendador debe bonificar al cliente por falla propia del equipo.

### Necesidad de orquestacion transaccional con disparadores duales

El sistema requiere dos tipos de disparadores para mantenimiento preventivo: por horometro (basado en horas de uso reportadas por telemetria) Y por calendario (basado en tiempo transcurrido desde el ultimo servicio), ejecutando la orden con lo que ocurra primero. Un equipo almacenado sin uso sigue necesitando servicio porque el aceite se degrada, las mangueras hidraulicas se resecan y las baterias se descargan. El modulo de mantenimiento no puede depender exclusivamente de la telemetria.

### El problema y el riesgo operativo

El riesgo principal radica en operar estos tres dominios (maquina, finanzas y seguridad) en silos aislados o depender de procesos manuales.

**Perdidas economicas:** Si la telemetria falla en conectarse con el modulo de finanzas, eventos criticos de dano en equipos costosos no se cobran oportunamente del deposito en garantia. Emitir comprobantes fiscales incorrectos en el ciclo de cobranza expone a la empresa a multas fiscales del SAT.

**Riesgos de seguridad y multas normativas:** Si un equipo con alertas de diagnostico (DTC/SPN) criticas no bloquea automaticamente su asignacion a una nueva renta, se pone en riesgo directo la vida del operador. Una inspeccion de la STPS que no encuentre la documentacion automatizada del programa de mantenimiento terminara en clausura o sancion severa.

**Necesidad de trazabilidad de extremo a extremo:** El ERP requiere orquestacion transaccional automatizada mediante maquinas de estado con garantia de completitud eventual. Un impacto registrado por sensor debe ser trazable hasta el PDF legal del protocolo LOTO y el timbrado final del CFDI de ingreso por penalizacion. La orquestacion no es sincronica (el timbrado ante el PAC puede fallar temporalmente), sino eventual con trazabilidad completa.

---

## 3. Decision: Esquema de Base de Datos (PostgreSQL + PostGIS)

### PROHIBICION ESTRICTA DE ARQUITECTURA (Flujos Fiscales)

A nivel sistemico, contable y fiscal queda estrictamente prohibido que los cargos por danos a la maquinaria sustituyan, modifiquen o cancelen la factura de renta original. La factura que ampara el periodo de arrendamiento y el cobro de una penalizacion por mal uso son eventos transaccionales fiscalmente independientes y bajo ninguna circunstancia deben mezclarse en el mismo comprobante.

El orquestador financiero operara exclusivamente bajo tres flujos mutuamente excluyentes:

| Escenario | Flujo Fiscal | Tipo CFDI | Impacto Contable |
|-----------|-------------|-----------|-----------------|
| Dano por negligencia del cliente | Cargo por Dano | NUEVO CFDI Ingreso (independiente) | Deposito migra de Pasivo a Ingreso |
| Error en factura original (RFC, monto, CP) | Sustitucion (Tipo 04) | CFDI Ingreso de sustitucion + Cancelacion (Motivo 01) | Sin afectacion a deposito |
| Falla del equipo imputable al arrendador | Bonificacion | CFDI Egreso / Nota de Credito (TipoRelacion 01) | Sin afectacion a deposito |

### 3.1 Tipos Enumerados

```sql
-- ============================================================================
-- ENUMS Y TIPOS PERSONALIZADOS
-- ============================================================================

CREATE TYPE equipment_type_enum AS ENUM (
    'MINIEXCAVATOR', 'SKID_STEER', 'SCISSOR_LIFT',
    'BOOM_LIFT', 'COMPACTOR', 'GENERATOR', 'PUMP'
);

CREATE TYPE equipment_status_enum AS ENUM (
    'AVAILABLE', 'RENTED', 'IN_MAINTENANCE', 'DECOMMISSIONED'
);

CREATE TYPE billing_model_enum AS ENUM (
    'FIXED_PERIOD', 'HOURLY_USAGE', 'TIERED', 'SHIFT_BASED'
);

CREATE TYPE contract_status_enum AS ENUM (
    'DRAFT', 'RESERVED', 'ACTIVE', 'SUSPENDED_MAINTENANCE',
    'RETURNING', 'INSPECTION', 'OVERDUE', 'IN_DISPUTE', 'CLOSED'
);

CREATE TYPE alert_type_enum AS ENUM (
    'FAULT_CODE', 'IMPACT', 'TILT', 'OVERTEMP',
    'OVERPRESSURE', 'GEOFENCE_BREACH'
);

CREATE TYPE severity_enum AS ENUM ('INFO', 'WARNING', 'CRITICAL', 'SHUTDOWN');

CREATE TYPE damage_attribution_enum AS ENUM (
    'NORMAL_WEAR', 'TENANT_ATTRIBUTABLE', 'ENVIRONMENTAL',
    'PRE_EXISTING', 'UNDER_DISPUTE'
);

CREATE TYPE maintenance_order_type_enum AS ENUM (
    'PREVENTIVE', 'CORRECTIVE', 'INSPECTION'
);

CREATE TYPE work_order_status_enum AS ENUM (
    'PENDING', 'IN_PROGRESS', 'LOTO_APPLIED', 'COMPLETED', 'CANCELLED'
);
```

### 3.2 Grupo 1: Telemetria y Equipos (ISO 15143-3 / AEMP 2.0)

```sql
-- ============================================================================
-- TABLA: equipment (Catalogo Maestro de Maquinaria)
-- Mapea los metadatos del EquipmentHeader del estandar ISO 15143-3.
-- El campo current_status es la state machine del activo fisico, operada
-- exclusivamente por el Worker 3 (mantenimiento). NUNCA debe ser consultada
-- por el Worker 7 (contrato) — ver Regla No-Join en Seccion 6.
-- ============================================================================
CREATE TABLE equipment (
    equipment_id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_tag       VARCHAR(50) UNIQUE NOT NULL,   -- Identificador fisico visible en el equipo
    serial_number   VARCHAR(100) UNIQUE NOT NULL,  -- ISO 15143-3: PIN / Numero de serie
    make            VARCHAR(50) NOT NULL,           -- ISO 15143-3: Marca del fabricante
    model           VARCHAR(50) NOT NULL,           -- ISO 15143-3: Modelo del equipo
    year            INT,                            -- Ano de fabricacion
    equipment_type  equipment_type_enum NOT NULL,   -- Clasificacion por tipo de maquinaria
    weight_tons     DECIMAL(6,2),                   -- Peso operativo en toneladas
    acquisition_cost DECIMAL(15,2),                 -- Costo de adquisicion (para calculo de ROI y deposito)
    acquisition_date DATE,

    -- STATE MACHINE DEL ACTIVO FISICO (independiente del contrato)
    current_status  equipment_status_enum NOT NULL DEFAULT 'AVAILABLE',

    -- INTEGRACION TELEMATICA
    aemp_endpoint_url VARCHAR(500),                 -- URL del proveedor telematico para polling AEMP 2.0

    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: equipment_load_profiles (Perfiles EFLH por Modelo y Aplicacion)
-- EFLH = (EngineHours x engine_load_factor) + (PTOHours x pto_load_factor)
-- Relacion por tipo/modelo, no por equipo individual. Un equipo referencia
-- su perfil via FK; se puede sobreescribir a nivel individual si es atipico.
-- ============================================================================
CREATE TABLE equipment_load_profiles (
    profile_id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_make      VARCHAR(50) NOT NULL,
    equipment_model     VARCHAR(50) NOT NULL,
    application_type    VARCHAR(50) NOT NULL,        -- Ej: demolicion, excavacion, carga general
    engine_load_factor  DECIMAL(5,3) NOT NULL,       -- Factor de carga del motor (0.000-1.000)
    pto_load_factor     DECIMAL(5,3) NOT NULL,       -- Factor de carga del PTO (0.000-1.000)
    maintenance_interval_hours INT,                   -- Intervalo base del fabricante (ej. 250h)
    target_utilization_pct DECIMAL(5,2),             -- Porcentaje de utilizacion objetivo
    last_updated        TIMESTAMPTZ DEFAULT NOW(),

    UNIQUE(equipment_make, equipment_model, application_type)
);

-- FK opcional en equipment para vincular perfil por defecto
ALTER TABLE equipment ADD COLUMN load_profile_id UUID REFERENCES equipment_load_profiles(profile_id);

-- ============================================================================
-- TABLA: equipment_thresholds (Umbrales Configurables por Tipo/Modelo)
-- Evita hardcodear umbrales de alerta. Un compactador vibratorio tiene
-- umbrales de impacto distintos a una plataforma de tijera.
-- ============================================================================
CREATE TABLE equipment_thresholds (
    threshold_id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_type  equipment_type_enum NOT NULL,
    equipment_model VARCHAR(50),                     -- NULL = aplica a todo el tipo
    threshold_type  VARCHAR(30) NOT NULL,            -- Ej: impact_g, tilt_lateral, coolant_temp, hydraulic_psi
    warning_value   DECIMAL(10,3),
    critical_value  DECIMAL(10,3),
    shutdown_value  DECIMAL(10,3),
    unit_of_measure VARCHAR(20) NOT NULL,            -- Ej: g, degrees, celsius, bar

    UNIQUE(equipment_type, equipment_model, threshold_type)
);

-- ============================================================================
-- TABLA: geofences (Poligonos de Obras Autorizadas)
-- Usa GEOGRAPHY(POLYGON) para calculo de distancias reales sobre curvatura
-- terrestre. Vinculado al contrato para validar ubicacion autorizada.
-- ============================================================================
CREATE TABLE geofences (
    geofence_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,               -- Nombre de la obra o sitio
    perimeter   GEOGRAPHY(POLYGON, 4326) NOT NULL,   -- Poligono en coordenadas WGS84
    is_yard     BOOLEAN DEFAULT FALSE,               -- TRUE si es patio del arrendador
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: telemetry_readings (Snapshots Telematicos ISO 15143-3)
-- Particionada por rango temporal. A 5 min/equipo, 100 equipos generan
-- ~28,800 filas/dia (~10M/ano). Particiones mensuales permiten DROP limpio
-- para retencion y queries optimizados por rango de fecha.
-- ============================================================================
CREATE TABLE telemetry_readings (
    snapshot_id     UUID DEFAULT gen_random_uuid(),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),

    -- METADATOS TEMPORALES (ISO 15143-3: Datetime)
    recorded_at     TIMESTAMPTZ NOT NULL,            -- Frecuencia: 5 min activo / 4h reposo

    -- DATOS GEOGRAFICOS (ISO 15143-3: Location)
    -- CEP50 ~ 3m, CEP95 ~ 6.2m. GEOGRAPHY calcula distancias reales.
    location        GEOGRAPHY(POINT, 4326) NOT NULL,
    altitude        DECIMAL(8,2),
    heading         DECIMAL(5,2),                    -- Rumbo en grados (0-360)
    speed_kmh       DECIMAL(6,2),                    -- Velocidad en km/h
    hdop            DECIMAL(4,2),                    -- Dilucion de Precision Horizontal
    satellites      INT,                             -- Numero de satelites en fix

    -- HOROMETROS SEPARADOS (ISO 15143-3)
    engine_hours                    DECIMAL(10,2),   -- Horas totales motor encendido
    pto_hours                       DECIMAL(10,2),   -- Horas toma de fuerza (PTO)
    idle_hours                      DECIMAL(10,2),   -- Inactividad ciclo actual
    cumulative_idle_hours           DECIMAL(10,2),   -- Idle con equipo operando
    cumulative_idle_non_operating_hours DECIMAL(10,2), -- Idle sin operacion efectiva

    -- METRICAS DE CONSUMO Y DISTANCIA (ISO 15143-3)
    distance        DECIMAL(10,2),                   -- Odometro acumulado
    fuel_used       DECIMAL(10,2),                   -- FuelUsed acumulado
    fuel_level      DECIMAL(5,2),                    -- FuelLevel (%)
    def_used        DECIMAL(10,2),                   -- DEFUsed (fluido escape diesel)
    def_level       DECIMAL(5,2),                    -- DEFLevel (%)
    engine_status   INT,                             -- 1=Running, 0=Stopped
    load_factor     DECIMAL(5,2),                    -- LoadFactor ISO 15143-3
    active_switches JSONB,                           -- Switches (estado de reles/circuitos)

    -- SENSORES COMPLEMENTARIOS (RentMaq Pro)
    impact_g_x          DECIMAL(5,2),                -- Sensor impacto eje X (+/-g)
    impact_g_y          DECIMAL(5,2),                -- Sensor impacto eje Y (+/-g)
    impact_g_z          DECIMAL(5,2),                -- Sensor impacto eje Z (+/-g)
    tilt_lateral        DECIMAL(5,2),                -- Inclinometro lateral (grados)
    tilt_longitudinal   DECIMAL(5,2),                -- Inclinometro longitudinal (grados)
    hydraulic_pressure  DECIMAL(10,2),               -- Presion hidraulica (bar)
    engine_temperature  DECIMAL(5,2),                -- Temp refrigerante motor (C)
    ambient_temperature DECIMAL(5,2),                -- Temp ambiente (C)

    PRIMARY KEY (snapshot_id, recorded_at)
) PARTITION BY RANGE (recorded_at);

-- Particion ejemplo para Abril 2026
CREATE TABLE telemetry_readings_2026_04 PARTITION OF telemetry_readings
    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');

-- Indices optimizados
CREATE INDEX idx_telemetry_location ON telemetry_readings USING GIST (location);
CREATE INDEX idx_telemetry_equipment_time ON telemetry_readings (equipment_id, recorded_at DESC);

-- ============================================================================
-- TABLA: telemetry_alerts (Eventos de Falla, Impacto, Inclinacion, Geofence)
-- alert_type determina que campos son relevantes. SPN/FMI solo aplican
-- cuando alert_type = 'FAULT_CODE'. Impacto/tilt tienen sus propios campos.
-- ============================================================================
CREATE TABLE telemetry_alerts (
    alert_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),
    snapshot_id     UUID,                            -- Referencia a telemetry_readings
    detected_at     TIMESTAMPTZ NOT NULL,

    -- TIPO Y SEVERIDAD
    alert_type      alert_type_enum NOT NULL,
    severity        severity_enum NOT NULL,

    -- PROTOCOLO J1939 (solo cuando alert_type = 'FAULT_CODE')
    spn             INT,                             -- Suspect Parameter Number (nullable)
    fmi             INT,                             -- Failure Mode Identifier (nullable)

    -- VALORES DE UMBRAL (para todas las alertas)
    threshold_value DECIMAL(10,3),                   -- Umbral configurado que se supero
    actual_value    DECIMAL(10,3),                   -- Valor real medido en el momento

    -- DATOS ESPECIFICOS POR TIPO DE ALERTA
    impact_g_x      DECIMAL(5,2),                    -- Solo para alert_type = 'IMPACT'
    impact_g_y      DECIMAL(5,2),
    impact_g_z      DECIMAL(5,2),
    tilt_lateral_deg    DECIMAL(5,2),                -- Solo para alert_type = 'TILT'
    tilt_longitudinal_deg DECIMAL(5,2),

    -- ATRIBUCION (alimentada por damage_assessments, no automatica)
    attributed_to_tenant BOOLEAN DEFAULT FALSE,

    -- FLUJO OPERATIVO
    description     TEXT,                            -- Descripcion legible de la falla
    acknowledged    BOOLEAN DEFAULT FALSE,
    acknowledged_by UUID,                            -- Supervisor que reviso la alerta
    acknowledged_at TIMESTAMPTZ,
    resolved        BOOLEAN DEFAULT FALSE
);

CREATE INDEX idx_alerts_equipment_severity ON telemetry_alerts (equipment_id, severity, resolved);
CREATE INDEX idx_alerts_detected ON telemetry_alerts (detected_at DESC);
```

### 3.3 Grupo 2: Mantenimiento y Cumplimiento NOM-004

```sql
-- ============================================================================
-- TABLA: maintenance_schedules (Intervalos Duales: Horas + Calendario)
-- Mapea intervalos reales de fabricantes con logica "lo que ocurra primero".
-- ============================================================================
CREATE TABLE maintenance_schedules (
    schedule_id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),

    -- CONFIGURACION DEL FABRICANTE
    service_tier    VARCHAR(50) NOT NULL,             -- Ej: 'Servicio_250h', 'Inspeccion_Anual'

    -- LOGICA DE DISPARADOR DUAL
    interval_hours  INT,                              -- Disparador por horometro (ej. 250)
    interval_days   INT,                              -- Disparador por calendario (ej. 180)

    -- TRACKING DE EJECUCION
    last_service_hours  DECIMAL(10,2),
    last_service_date   DATE,

    -- CALCULO DINAMICO (Worker 3 dispara cuando se cumple CUALQUIERA)
    next_due_hours  DECIMAL(10,2),
    next_due_date   DATE,

    is_active       BOOLEAN DEFAULT TRUE,
    created_at      TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: maintenance_work_orders (Ordenes de Trabajo / Bitacora NOM-004)
-- Bitacora legal auditable. Retencion minima: 12 meses (NOM-004 Art. 7.2.3).
-- Campos LOTO y protectors son NOT NULL al pasar a COMPLETED.
-- ============================================================================
CREATE TABLE maintenance_work_orders (
    work_order_id   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),
    schedule_id     UUID REFERENCES maintenance_schedules(schedule_id),
    linked_alert_id UUID REFERENCES telemetry_alerts(alert_id),

    -- METADATOS OPERATIVOS
    order_type      maintenance_order_type_enum NOT NULL,
    status          work_order_status_enum NOT NULL DEFAULT 'PENDING',

    -- ORIGEN DEL DISPARADOR
    trigger_source  VARCHAR(30) NOT NULL
        CHECK (trigger_source IN (
            'TELEMETRY_AUTO', 'CALENDAR_AUTO', 'MANUAL_INSPECTION', 'TENANT_REPORT'
        )),

    -- TRAZABILIDAD TEMPORAL (NOM-004 Art. 7.2.3)
    execution_date  DATE NOT NULL,

    -- PROTOCOLO LOTO - NOM-004 Art. 7.2.2 c)
    loto_applied            BOOLEAN DEFAULT FALSE,
    loto_applied_at         TIMESTAMPTZ,
    loto_timeout_hours      INT DEFAULT 24,          -- Timeout configurable para alerta de supervisor

    -- REINSTALACION DE PROTECTORES - NOM-004 Art. 7.2.2 a)
    protectors_reinstalled  BOOLEAN DEFAULT FALSE,
    protectors_verified_at  TIMESTAMPTZ,

    -- DETALLES DE TRABAJO
    technician_notes        TEXT,
    performed_by_worker_id  UUID,                    -- ID del tecnico capacitado
    parts_used              JSONB,                   -- [{part_number, description, qty, cost}]
    labor_hours             DECIMAL(5,2),
    total_cost              DECIMAL(15,2),

    -- PROXIMO SERVICIO
    next_service_due_at     DATE,
    next_service_due_hours  DECIMAL(10,2),

    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_work_orders_equipment_date ON maintenance_work_orders(equipment_id, execution_date);
CREATE INDEX idx_work_orders_status ON maintenance_work_orders(status) WHERE status != 'COMPLETED';

-- ============================================================================
-- TABLA: equipment_certifications (Certificaciones y Bloqueos Operativos)
-- Critico para plataformas de elevacion (ANSI A92 via NOM-009-STPS-2011).
-- Un equipo con certificacion vencida NO puede transicionar a 'AVAILABLE'.
-- ============================================================================
CREATE TABLE equipment_certifications (
    certification_id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id        UUID NOT NULL REFERENCES equipment(equipment_id),
    certification_name  VARCHAR(100) NOT NULL,        -- Ej: 'ANSI A92.22 Anual'
    certification_type  VARCHAR(50) NOT NULL,          -- Ej: 'annual_inspection', 'pre_delivery'
    issued_by           VARCHAR(200),                  -- Unidad de verificacion o tecnico
    issue_date          DATE NOT NULL,
    expiration_date     DATE NOT NULL,
    blocks_rental       BOOLEAN DEFAULT TRUE,          -- Certificacion vencida impide renta
    document_url        VARCHAR(500),                  -- Ruta al PDF firmado
    status              VARCHAR(20) NOT NULL
        CHECK (status IN ('VALID', 'EXPIRED', 'REVOKED')),
    created_at          TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_certifications_expiration ON equipment_certifications(expiration_date)
    WHERE status = 'VALID';

-- ============================================================================
-- TABLA: operator_certifications (Constancias DC-3 de Operadores)
-- NOM-004 exige que operadores esten capacitados en el uso del equipo.
-- Vinculado al contrato: el arrendatario designa al operador.
-- ============================================================================
CREATE TABLE operator_certifications (
    operator_cert_id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id         UUID,                         -- FK se agrega despues de crear rental_contracts
    operator_name       VARCHAR(200) NOT NULL,
    dc3_certificate_number VARCHAR(100),              -- Numero de constancia DC-3
    equipment_type_certified equipment_type_enum NOT NULL,
    issued_at           DATE NOT NULL,
    expires_at          DATE,
    document_url        VARCHAR(500),
    created_at          TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: inspection_checklists (Checklists Pre-Entrega y Post-Devolucion)
-- Registro completo de inspeccion, no solo hallazgos negativos.
-- Linea base fotografica pre-entrega vs post-devolucion.
-- ============================================================================
CREATE TABLE inspection_checklists (
    checklist_id    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),
    contract_id     UUID,                             -- FK se agrega despues de crear rental_contracts
    inspection_type VARCHAR(20) NOT NULL
        CHECK (inspection_type IN ('PRE_DELIVERY', 'POST_RETURN')),
    inspector_id    UUID NOT NULL,

    -- ITEMS ESTRUCTURADOS
    checklist_items JSONB NOT NULL,                   -- [{item, category, result: pass/fail/na, notes}]
    photos          JSONB,                            -- [{url, description, timestamp}]

    -- RESULTADO GLOBAL
    overall_result  VARCHAR(30) NOT NULL
        CHECK (overall_result IN ('APPROVED', 'APPROVED_WITH_OBSERVATIONS', 'REJECTED')),

    -- METRICAS AL MOMENTO DE INSPECCION
    horometer_reading   DECIMAL(10,2),
    fuel_level_pct      DECIMAL(5,2),

    inspected_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at      TIMESTAMPTZ DEFAULT NOW()
);
```

### 3.4 Grupo 3: Financiero y Contractual (CFDI 4.0 / NIF D-5)

```sql
-- ============================================================================
-- TABLA: tenants (Arrendatarios / Clientes)
-- Campos alineados a requisitos estrictos CFDI 4.0.
-- ============================================================================
CREATE TABLE tenants (
    tenant_id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    legal_name      VARCHAR(255) NOT NULL,
    rfc             VARCHAR(13) UNIQUE NOT NULL,
    tax_regime      VARCHAR(3) NOT NULL,              -- Catalogo SAT (601 General de Ley, etc.)
    postal_code     VARCHAR(5) NOT NULL,              -- Requisito estricto CFDI 4.0
    uso_cfdi_default VARCHAR(4) DEFAULT 'G03',        -- G03 Gastos en general, I01 Construcciones
    credit_limit    DECIMAL(15,2),
    credit_status   VARCHAR(30) DEFAULT 'PENDING_EVALUATION'
        CHECK (credit_status IN (
            'PENDING_EVALUATION', 'APPROVED', 'SUSPENDED', 'BLOCKED'
        )),
    created_at      TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: rental_contracts (Contratos de Arrendamiento)
-- State machine de 9 estados. Operada exclusivamente por Worker 7.
-- NUNCA debe consultar equipment.current_status — ver Regla No-Join.
-- ============================================================================
CREATE TABLE rental_contracts (
    contract_id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       UUID NOT NULL REFERENCES tenants(tenant_id),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),

    -- STATE MACHINE DEL CONTRATO (9 estados)
    status          contract_status_enum NOT NULL DEFAULT 'DRAFT',

    -- PERIODO
    start_date      DATE NOT NULL,
    end_date        DATE,

    -- MODELO DE FACTURACION
    billing_model   billing_model_enum NOT NULL DEFAULT 'FIXED_PERIOD',
    base_monthly_rate   DECIMAL(15,2) NOT NULL,
    daily_rate          DECIMAL(15,2),
    weekly_rate         DECIMAL(15,2),
    overtime_multiplier DECIMAL(5,2),                 -- Multiplicador para horas extra (ej. 1.5)
    max_hours_included  INT,                          -- Horas incluidas antes de overtime (ej. 176/mes)

    -- DEPOSITO Y SEGURO
    deposit_type    VARCHAR(50),                      -- CASH, CHECK, CREDIT_CARD_PREAUTH, PROMISSORY_NOTE, SURETY_BOND
    insurance_policy_number VARCHAR(100),              -- Poliza de seguro vigente

    -- GEOFENCE
    geofence_id     UUID REFERENCES geofences(geofence_id),

    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);

-- Agregar FKs diferidas a tablas que dependen de rental_contracts
ALTER TABLE operator_certifications
    ADD CONSTRAINT fk_operator_cert_contract
    FOREIGN KEY (contract_id) REFERENCES rental_contracts(contract_id);

ALTER TABLE inspection_checklists
    ADD CONSTRAINT fk_checklist_contract
    FOREIGN KEY (contract_id) REFERENCES rental_contracts(contract_id);

-- ============================================================================
-- TABLA: cfdi_documents (Comprobantes Fiscales CFDI 4.0)
-- Creada antes de deposits para resolver dependencia circular.
-- Trazabilidad completa del ciclo de vida del comprobante ante el SAT.
-- ============================================================================
CREATE TABLE cfdi_documents (
    cfdi_id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id     UUID REFERENCES rental_contracts(contract_id),

    -- TIPO Y METODO
    cfdi_type       VARCHAR(1) NOT NULL
        CHECK (cfdi_type IN ('I', 'E', 'P')),       -- Ingreso, Egreso, Pago
    payment_method  VARCHAR(3) NOT NULL
        CHECK (payment_method IN ('PUE', 'PPD')),

    -- FOLIO FISCAL
    uuid_fiscal     UUID UNIQUE,                      -- UUID del SAT (post-timbrado)
    total_amount    DECIMAL(15,2) NOT NULL,

    -- CATALOGOS SAT OBLIGATORIOS
    uso_cfdi        VARCHAR(4) NOT NULL,              -- G03, I01, G02, etc.
    forma_pago      VARCHAR(2),                       -- 01 efectivo, 03 transferencia, 99 por definir

    -- RELACIONES PARA SUSTITUCION Y CANCELACION
    related_cfdi_id UUID REFERENCES cfdi_documents(cfdi_id),
    relation_type   VARCHAR(2),                       -- '04' Sustitucion, '01' Nota de credito
    cancel_reason   VARCHAR(2),                       -- '01' Con errores con relacion

    -- TRAZABILIDAD PAC
    cancellation_status VARCHAR(50),                  -- 'Cancelable sin aceptacion', 'En proceso', 'Rechazada'
    xml_url         VARCHAR(500),                     -- Ruta al XML timbrado (S3/Azure Blob)
    pdf_url         VARCHAR(500),                     -- Ruta al PDF generado
    pac_provider    VARCHAR(100),                     -- Proveedor Autorizado de Certificacion

    -- ESTADO INTERNO
    status          VARCHAR(20) NOT NULL
        CHECK (status IN ('TIMBRADO', 'CANCELADO', 'PENDIENTE_PAGO', 'PAGADO')),
    issued_at       TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: deposits (Depositos en Garantia)
-- Lifecycle contable: Pasivo -> Ingreso (NIF D-5 / Resolucion 42/IVA/2019-RF)
-- ============================================================================
CREATE TABLE deposits (
    deposit_id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id     UUID NOT NULL REFERENCES rental_contracts(contract_id),
    amount          DECIMAL(15,2) NOT NULL,           -- Monto original recibido

    -- TRAZABILIDAD CONTABLE
    applied_amount  DECIMAL(15,2) NOT NULL DEFAULT 0, -- Porcion transformada en Ingreso
    refunded_amount DECIMAL(15,2) NOT NULL DEFAULT 0, -- Porcion devuelta al cliente
    related_cfdi_id UUID REFERENCES cfdi_documents(cfdi_id), -- CFDI que ampara la aplicacion

    -- STATE MACHINE
    status          VARCHAR(30) NOT NULL
        CHECK (status IN (
            'PENDING_COLLECTION', 'HELD_AS_LIABILITY',
            'PARTIALLY_APPLIED', 'FULLY_APPLIED', 'REFUNDED'
        )),

    -- CLASIFICACION CONTABLE SAT
    accounting_classification VARCHAR(20) NOT NULL DEFAULT 'LIABILITY'
        CHECK (accounting_classification IN ('LIABILITY', 'RECOGNIZED_INCOME')),

    received_at     TIMESTAMPTZ,

    -- CONSTRAINT: no se puede aplicar/reembolsar mas de lo recibido
    CONSTRAINT chk_deposit_balance CHECK (applied_amount + refunded_amount <= amount)
);

-- ============================================================================
-- TABLA: damage_assessments (Evaluaciones de Dano - Puente Patio/Finanzas)
-- Eslabon intermedio que requiere validacion humana antes de generar cargo.
-- Cubre tanto danos detectados por telemetria como hallazgos fisicos
-- en inspeccion post-devolucion que no generan alertas telematricas.
-- ============================================================================
CREATE TABLE damage_assessments (
    assessment_id   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id     UUID NOT NULL REFERENCES rental_contracts(contract_id),
    equipment_id    UUID NOT NULL REFERENCES equipment(equipment_id),

    -- RELACIONES TECNICAS (ambas opcionales)
    fault_event_id  UUID REFERENCES telemetry_alerts(alert_id),
    checklist_id    UUID REFERENCES inspection_checklists(checklist_id),
    work_order_id   UUID REFERENCES maintenance_work_orders(work_order_id),

    -- INSPECCION
    inspection_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    assessor_id     UUID NOT NULL,
    damage_description TEXT NOT NULL,

    -- CLASIFICACION DE DANO (reemplaza booleano is_chargeable)
    attribution     damage_attribution_enum NOT NULL DEFAULT 'UNDER_DISPUTE',

    -- IMPACTO FINANCIERO
    estimated_repair_cost DECIMAL(15,2),

    -- EVIDENCIA Y COMPLIANCE
    customer_signature_url VARCHAR(500),
    photos          JSONB,                            -- [{url, description, timestamp}]

    -- STATE MACHINE
    status          VARCHAR(50) NOT NULL
        CHECK (status IN ('DRAFT', 'UNDER_REVIEW', 'APPROVED_FOR_CHARGE', 'DISMISSED')),

    created_at      TIMESTAMPTZ DEFAULT NOW(),
    updated_at      TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TABLA: extraordinary_charges (Cargos Extraordinarios por Dano)
-- Conecta telemetria -> evaluacion -> deposito -> CFDI.
-- Maneja el caso donde repair_cost > deposit_amount.
-- ============================================================================
CREATE TABLE extraordinary_charges (
    charge_id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contract_id     UUID NOT NULL REFERENCES rental_contracts(contract_id),
    deposit_id      UUID REFERENCES deposits(deposit_id),
    assessment_id   UUID REFERENCES damage_assessments(assessment_id),
    cfdi_id         UUID REFERENCES cfdi_documents(cfdi_id),

    -- STATE MACHINE
    status          VARCHAR(30) NOT NULL
        CHECK (status IN ('DETECTED', 'ATTRIBUTED', 'APPLIED_TO_DEPOSIT', 'INVOICED')),

    amount              DECIMAL(15,2) NOT NULL,       -- Monto total del cargo
    amount_from_deposit DECIMAL(15,2) DEFAULT 0,      -- Porcion cubierta por deposito
    amount_direct_bill  DECIMAL(15,2) DEFAULT 0,      -- Porcion facturada directamente (excedente)
    reason              TEXT NOT NULL,

    created_at      TIMESTAMPTZ DEFAULT NOW()
);
```

---

## 4. Decision: Workers de Orquestacion

Cada worker se implementa como un .NET Background Service (`IHostedService`) y se documenta como un Conductor Track independiente con su propio `spec.md` y `plan.md`.

### REGLA DE DESACOPLAMIENTO (No-Join Rule)

Queda explicitamente prohibido a nivel de codigo fuente que cualquier Worker del dominio contractual/financiero (Workers 4, 5, 7) consulte `equipment.current_status`, y que cualquier Worker del dominio operativo/mantenimiento (Workers 2, 3, 6) consulte `rental_contracts.status`. La comunicacion entre ambas maquinas de estado se realiza exclusivamente mediante eventos: insercion de registros en tablas intermedias (`telemetry_alerts`, `damage_assessments`, `extraordinary_charges`). No deben existir JOINs directos ni inyeccion de repositorios cruzados entre dominios.

### Worker 1: telemetry-ingestion-worker

**Proposito:** Ingesta de datos AEMP 2.0 desde proveedores telematicos.

**Disparador:** Cron job cada 5 minutos.

**Flujo:**
1. Lee `equipment` filtrando por `current_status IN ('RENTED', 'AVAILABLE')` y `aemp_endpoint_url IS NOT NULL`.
2. Hace polling HTTP al endpoint AEMP 2.0 de cada equipo.
3. Parsea el payload ISO 15143-3, normaliza unidades al esquema de `telemetry_readings`.
4. Inserta batch en `telemetry_readings` (tabla particionada).
5. Para cada lectura, ejecuta validacion espacial contra `geofences` usando `ST_Covers(perimeter, location)`. Si el equipo esta fuera del poligono autorizado, inserta alerta `GEOFENCE_BREACH` en `telemetry_alerts`. NO transiciona el contrato (ADR-003 Seccion 2).

**Tablas:** Lee `equipment`, `geofences`. Escribe `telemetry_readings`, `telemetry_alerts`.

**SLA:** Toda la flota procesada en menos de 5 minutos por ciclo.

**Errores:** Si el endpoint AEMP no responde, registra el fallo y reintenta en el siguiente ciclo. No bloquea el procesamiento de otros equipos.

### Worker 2: alert-evaluation-worker

**Proposito:** Evalua cada lectura nueva contra umbrales configurados y genera alertas.

**Disparador:** Event-driven (nuevo registro en `telemetry_readings`).

**Flujo:**
1. Lee la lectura nueva y obtiene el `equipment_type` del equipo.
2. Consulta `equipment_thresholds` para ese tipo/modelo.
3. Compara valores de impacto, inclinacion, temperatura, presion contra umbrales WARNING/CRITICAL/SHUTDOWN.
4. Para alertas de tipo `FAULT_CODE`, parsea SPN+FMI del protocolo J1939.
5. Implementa logica de perfiles de vibracion base: un compactador en operacion normal genera vibraciones que serian alarma en una plataforma. Filtra usando `equipment_load_profiles` para evitar falsos positivos.
6. Inserta alertas en `telemetry_alerts` con `threshold_value` y `actual_value` documentados.
7. Para alertas CRITICAL o SHUTDOWN, publica evento para que Worker 3 genere orden de mantenimiento inmediata.

**Tablas:** Lee `telemetry_readings`, `equipment`, `equipment_thresholds`, `equipment_load_profiles`. Escribe `telemetry_alerts`.

**SLA:** Evaluacion completada en menos de 30 segundos por lectura.

### Worker 3: maintenance-orchestration-worker

**Proposito:** Genera ordenes de mantenimiento preventivo y correctivo. Opera la state machine de `equipment.current_status`.

**Disparadores:**
- Reactivo: alerta CRITICAL/SHUTDOWN del Worker 2.
- Preventivo: evaluacion periodica (cada hora) contra `maintenance_schedules`.

**Flujo Correctivo:**
1. Recibe evento de alerta critica.
2. Crea `maintenance_work_orders` con `trigger_source = 'TELEMETRY_AUTO'`, `linked_alert_id` apuntando a la alerta.
3. Transiciona `equipment.current_status` a `IN_MAINTENANCE`.
4. Publica evento indicando que un equipo en renta entro a mantenimiento (Worker 7 lo consume para transicionar contrato a `SUSPENDED_MAINTENANCE` si aplica).

**Flujo Preventivo (Disparador Dual):**
1. Cada hora, evalua todos los equipos activos contra `maintenance_schedules`.
2. Compara `engine_hours` actual (ultima lectura en `telemetry_readings`) contra `next_due_hours`.
3. Compara fecha actual contra `next_due_date`.
4. Si CUALQUIERA de los dos umbrales se cumple, genera orden preventiva.
5. Actualiza `next_due_hours` y `next_due_date` tras completar el servicio.

**Tablas:** Lee `telemetry_readings`, `telemetry_alerts`, `maintenance_schedules`. Escribe `maintenance_work_orders`, `equipment` (solo `current_status`), `maintenance_schedules`.

**SLA:** Ordenes correctivas generadas en menos de 2 minutos desde la alerta. Evaluacion preventiva completada en menos de 10 minutos para toda la flota.

### Worker 4: financial-impact-worker

**Proposito:** Procesa el impacto financiero cuando un equipo en renta entra a mantenimiento o se detecta dano.

**Disparador:** Event-driven (nueva `damage_assessment` con status `APPROVED_FOR_CHARGE`, o evento de suspension de mantenimiento).

**Flujo por Dano Atribuido al Arrendatario:**
1. Lee `damage_assessments` con `attribution = 'TENANT_ATTRIBUTABLE'` y `status = 'APPROVED_FOR_CHARGE'`.
2. Crea registro en `extraordinary_charges` con `status = 'ATTRIBUTED'`.
3. Evalua `estimated_repair_cost` contra `deposits.amount - deposits.applied_amount`:
   - Si deposito cubre el monto: `amount_from_deposit = repair_cost`, `amount_direct_bill = 0`.
   - Si deposito NO cubre: `amount_from_deposit = saldo_disponible`, `amount_direct_bill = repair_cost - saldo_disponible`.
4. Actualiza `deposits.applied_amount`, migra `accounting_classification` a `RECOGNIZED_INCOME`.
5. Transiciona `extraordinary_charges.status` a `APPLIED_TO_DEPOSIT`.
6. Publica evento para Worker 5 solicitando emision de CFDI de Ingreso nuevo e independiente.
7. NUNCA toca, sustituye ni cancela la factura mensual de renta original.

**Flujo por Falla del Arrendador:**
1. Si la falla no es atribuible al cliente (`attribution IN ('NORMAL_WEAR', 'ENVIRONMENTAL')`).
2. Calcula credito proporcional por dias sin equipo.
3. Publica evento para Worker 5 solicitando CFDI de Egreso (Nota de Credito) con TipoRelacion 01.

**Tablas:** Lee `damage_assessments`, `deposits`, `rental_contracts`. Escribe `extraordinary_charges`, `deposits`.

### Worker 5: cfdi-lifecycle-worker

**Proposito:** Gestiona todo el ciclo de vida de documentos fiscales. Integra con PAC.

**Disparador:** Event-driven (solicitudes de emision de otros Workers) + Cron para facturacion periodica.

**Flujos:**

**Emision de CFDI de Ingreso (Renta Mensual):**
1. Cron mensual. Lee contratos `ACTIVE` con su `billing_model`.
2. Calcula monto segun modelo: `FIXED_PERIOD` usa `base_monthly_rate`; `TIERED` calcula horas de `telemetry_readings` y aplica `overtime_multiplier` sobre `max_hours_included`; `HOURLY_USAGE` multiplica horas reales por tarifa.
3. Timbra con PAC. Almacena XML/PDF. Registra en `cfdi_documents` con `payment_method = 'PPD'`.

**Emision de CFDI de Ingreso (Cargo por Dano):**
1. Recibe evento del Worker 4.
2. Emite CFDI de Ingreso NUEVO e independiente por concepto de penalizacion.
3. Vincula a `extraordinary_charges.cfdi_id`. Transiciona cargo a `INVOICED`.

**Emision de CFDI de Egreso (Bonificacion):**
1. Recibe evento del Worker 4.
2. Emite CFDI de Egreso con `relation_type = '01'` apuntando a factura original.

**Sustitucion por Error:**
1. Solo cuando se detecta error real en factura emitida (RFC, monto, concepto).
2. Cancela CFDI original con `cancel_reason = '01'`.
3. Reexpide con `relation_type = '04'`.

**Complementos de Pago (PPD):**
1. Al registrarse un pago parcial o total contra una factura PPD.
2. Emite CFDI tipo `P` vinculado a la factura PPD original.
3. Aplica a facturas de renta y a CFDIs de cargo extraordinario emitidos como PPD.

**Monitoreo de Cancelacion:**
1. Polling periodico al PAC para CFDIs con `cancellation_status = 'En proceso'`.
2. Actualiza status cuando el SAT acepta o rechaza la cancelacion.

**Tablas:** Lee `rental_contracts`, `tenants`, `telemetry_readings` (calculo de horas), `extraordinary_charges`. Escribe `cfdi_documents`.

**SLA:** Timbrado completado en menos de 60 segundos. Reintentos automaticos si el PAC no responde.

### Worker 6: compliance-document-worker

**Proposito:** Genera automaticamente PDFs auditables para cumplimiento NOM-004-STPS-1999.

**Disparador:** Event-driven (orden de trabajo completada) + Cron diario para alertas de vencimiento.

**Documentos que genera:**
1. **Bitacora Historica de Mantenimiento:** Compila desde `maintenance_work_orders`. Retencion minima 12 meses (NOM-004 Art. 7.2.3).
2. **Protocolo LOTO:** Certifica bloqueo de energia con `loto_applied = TRUE` y timestamp.
3. **Certificado de Reinstalacion de Protectores:** Verifica `protectors_reinstalled = TRUE`.
4. **Registro de Constancias DC-3:** Compila desde `operator_certifications`.
5. **Alerta de Vencimiento:** Cron diario verifica `equipment_certifications.expiration_date`. Notifica a 30 dias de expirar. Bloquea transicion a `AVAILABLE` si expira.

**Timeout de Seguridad LOTO:**
- Monitorea ordenes en estado `LOTO_APPLIED`.
- Si `NOW() - loto_applied_at > loto_timeout_hours`, alerta al supervisor.
- NO desbloquea automaticamente (violaria NOM-004). Solo escala.

**Tablas:** Lee `maintenance_work_orders`, `equipment_certifications`, `operator_certifications`, `inspection_checklists`. Genera PDFs a storage.

### Worker 7: contract-lifecycle-worker

**Proposito:** Orquesta las transiciones de la state machine del contrato (9 estados).

**Disparador:** Event-driven (eventos de otros Workers) + acciones de usuario.

**Transiciones:**

| Desde | Hacia | Disparador |
|-------|-------|-----------|
| DRAFT | RESERVED | Validacion crediticia aprobada |
| RESERVED | ACTIVE | Deposito recibido + Checklist pre-entrega aprobado + Operador DC-3 registrado |
| ACTIVE | SUSPENDED_MAINTENANCE | Evento Worker 3 (equipo entro a mantenimiento) |
| SUSPENDED_MAINTENANCE | ACTIVE | Evento Worker 6 (LOTO + protectores verificados) |
| ACTIVE | RETURNING | Accion humana: cliente en Portal o chofer en App. NUNCA geofence automatico. |
| ACTIVE | RETURNING | Dia 60 de mora: unica excepcion de RETURNING automatico (recoleccion forzada). |
| RETURNING | INSPECTION | Check-in fisico en patio (GPS corroborativo, no trigger). |
| INSPECTION | CLOSED | Sin hallazgos. Deposito liberado (REFUNDED). |
| INSPECTION | IN_DISPUTE | Danos con `attribution = 'TENANT_ATTRIBUTABLE'`. |
| ACTIVE | OVERDUE | Incumplimiento de pago detectado. |
| IN_DISPUTE | CLOSED | Cargo procesado (INVOICED) y pagado. |
| OVERDUE | CLOSED | Deuda saldada. |

**Regla OVERDUE:** Bloquea CLOSED (no libera deposito). Permite transiciones operativas. El adeudo NO secuestra mantenimiento ni recuperacion del activo.

**Escalacion Aging:**

| Dia | Accion |
|-----|--------|
| 30 | Notificacion. Restriccion de equipo adicional al tenant. |
| 60 | Recoleccion forzada. ACTIVE/OVERDUE -> RETURNING. |
| 90 | Bloqueo juridico. Provision cuentas incobrables (CNBV). |

**Tablas:** Lee `rental_contracts`, `deposits`, `inspection_checklists`, `damage_assessments`, `extraordinary_charges`. Escribe `rental_contracts` (solo `status`).

---

## 5. Maquinas de Estado Concurrentes (ADR-003)

### Principio: Independencia Total Fisico vs. Financiero

`equipment.current_status` y `rental_contracts.status` operan como procesos paralelos independientes. Un equipo puede estar `AVAILABLE` (reparado) mientras su contrato anterior esta en `IN_DISPUTE`.

`IN_DISPUTE` es un Financial Lock, NO un estado operativo. Esto permite que el activo siga generando ingresos en otro contrato mientras la SAGA financiera procesa el cobro.

### State Machine: equipment.current_status

```
AVAILABLE --> RENTED              (contrato activado)
RENTED --> IN_MAINTENANCE         (Worker 3: alerta critica o preventivo)
IN_MAINTENANCE --> AVAILABLE      (Worker 6: LOTO + protectores + certificacion vigente)
AVAILABLE --> DECOMMISSIONED      (baja definitiva)
IN_MAINTENANCE --> DECOMMISSIONED (dano irreparable)
```

**Bloqueo por certificacion:** Equipo con `equipment_certifications.status = 'EXPIRED'` y `blocks_rental = TRUE` NO puede transicionar a `AVAILABLE`.

### State Machine: rental_contracts.status

```
DRAFT --> RESERVED --> ACTIVE
ACTIVE --> SUSPENDED_MAINTENANCE --> ACTIVE
ACTIVE --> RETURNING --> INSPECTION --> CLOSED
ACTIVE --> OVERDUE (coexiste con transiciones operativas)
INSPECTION --> IN_DISPUTE --> CLOSED
OVERDUE --> CLOSED (solo al saldar deuda)
```

### State Machine: deposits.status

```
PENDING_COLLECTION --> HELD_AS_LIABILITY   (Pasivo sin IVA/ISR)
HELD_AS_LIABILITY --> PARTIALLY_APPLIED    (cargo parcial, migra a Ingreso)
HELD_AS_LIABILITY --> FULLY_APPLIED        (cargo consume todo)
PARTIALLY_APPLIED --> FULLY_APPLIED        (cargos adicionales)
HELD_AS_LIABILITY --> REFUNDED             (devolucion sin hallazgos)
PARTIALLY_APPLIED --> REFUNDED             (devolucion del remanente)
```

### State Machine: extraordinary_charges.status

```
DETECTED --> ATTRIBUTED            (telemetria + evaluacion confirman negligencia)
ATTRIBUTED --> APPLIED_TO_DEPOSIT  (deduccion contable ejecutada)
APPLIED_TO_DEPOSIT --> INVOICED    (CFDI de Ingreso timbrado)
```

---

## 6. Reglas de Implementacion Tecnica

### 6.1 Desacoplamiento (No-Join Rule)
Workers financieros (4, 5, 7) NO consultan `equipment.current_status`. Workers operativos (2, 3, 6) NO consultan `rental_contracts.status`. Comunicacion solo via eventos y tablas intermedias.

### 6.2 Timeout LOTO (NOM-004)
Orden en `LOTO_APPLIED` que excede `loto_timeout_hours` dispara alerta al supervisor. NO desbloquea automaticamente.

### 6.3 Geofencing = Alerta, no Trigger
Cruce de poligono genera `GEOFENCE_BREACH` en `telemetry_alerts`. Transicion a `RETURNING` requiere accion humana. Justificacion: HDOP/PDOP.

### 6.4 Escalacion Mora
Dia 30: notificacion. Dia 60: recoleccion forzada. Dia 90: cobranza juridica + provision CNBV.

### 6.5 Flujos Fiscales Excluyentes
Cargo por Dano (CFDI Ingreso nuevo), Sustitucion (Tipo 04 + Motivo 01), Bonificacion (CFDI Egreso, TipoRelacion 01). Nunca se mezclan.

### 6.6 Complementos PPD
Toda factura PPD requiere CFDI tipo P al registrar pagos. Aplica a renta y cargos extraordinarios.

---

## 7. Consecuencias y Trade-offs

### Positivas
- Trazabilidad completa sensor-a-CFDI.
- Cumplimiento automatizado: NOM-004, NOM-009, CFDI 4.0.
- Activo fisico sigue generando ingresos durante disputas financieras.
- Disparadores duales eliminan riesgo de equipo sin mantenimiento.
- Umbrales configurables eliminan falsos positivos.

### Negativas y Mitigaciones

| Riesgo | Mitigacion |
|--------|-----------|
| Complejidad 7 workers | Cada worker = Conductor Track con spec.md y criterios de aceptacion. |
| Idempotencia requerida | UUIDs como claves. Re-procesar no genera duplicados. |
| Fallo a mitad de cadena | Completitud eventual + reintentos + dead letter queue. |
| telemetry_readings crece rapido | Particionamiento mensual + DROP para retencion. |
| GPS HDOP falsos positivos | Geofence degradado a alerta. Perfiles vibracion para compactadores. |
| LOTO timeout bloquea equipos | Alerta supervisor sin desbloqueo automatico. |

---

## 8. Alternativas Consideradas

| Alternativa | Razon de Descarte |
|-------------|-------------------|
| Event Sourcing completo | Excesivo para MVP. State machines cubren trazabilidad requerida. |
| CQRS estricto con BD lectura separada | Carga de lectura del MVP no lo justifica. Agregable despues. |
| Saga distribuido | Monolito modular usa transacciones locales, no requiere compensacion distribuida. |
| Geofence automatico para RETURNING | Descartado por HDOP/PDOP. Degradado a alerta + confirmacion humana. |
| Booleano is_chargeable | Reemplazado por enum attribution (5 valores) para granularidad. |

---

## 9. Formato Conductor Track

### Resumen Ejecutivo (metadata.json)

ADR-001 define el esquema PostgreSQL/PostGIS y 7 workers .NET para orquestar el ciclo de vida de maquinaria rentada a traves de tres dominios: telemetria ISO 15143-3, mantenimiento NOM-004-STPS, y facturacion CFDI 4.0. State machines concurrentes desacopladas, disparadores duales, umbrales configurables, y tres flujos fiscales mutuamente excluyentes. Geofencing opera como alerta, no como trigger.

### Criterios de Aceptacion (/conductor:review)

1. **Integridad referencial:** Todas las FKs validas. No hay equipment_id huerfanos.
2. **No-Join Rule:** Workers financieros no consultan `equipment.current_status`. Workers operativos no consultan `rental_contracts.status`. Verificable con grep.
3. **Flujos fiscales separados:** Tests del Worker 5 demuestran que cargo por dano NUNCA cancela ni sustituye factura mensual.
4. **Disparador dual:** Tests del Worker 3 verifican generacion de orden por horometro Y por calendario (lo que ocurra primero).
5. **LOTO Gatekeeper:** Equipo en `IN_MAINTENANCE` NO transiciona a `AVAILABLE` sin `loto_applied = TRUE` Y `protectors_reinstalled = TRUE`.
6. **Certificacion ANSI A92:** Plataforma con certificacion `EXPIRED` NO transiciona a `AVAILABLE`. Test de regresion obligatorio.
7. **Geofence degradado:** Salida del poligono genera alerta `GEOFENCE_BREACH`, NUNCA transiciona contrato a `RETURNING`.
