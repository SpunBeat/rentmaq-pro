-- =============================================================================
-- RentMaq Pro — Post-Migration: Particionamiento de telemetry_readings
-- Ejecutar DESPUES de dotnet ef database update.
-- EF Core no soporta PARTITION BY RANGE nativamente.
--
-- Este script:
--   1. Renombra la tabla creada por EF Core
--   2. La recrea como tabla particionada
--   3. Genera 12 particiones mensuales hacia adelante
--   4. Migra datos existentes
--   5. Recrea indices
--   6. Elimina la tabla original
--
-- Idempotente: verifica si la tabla ya esta particionada antes de actuar.
-- =============================================================================

DO $$
DECLARE
    is_partitioned BOOLEAN;
    partition_start DATE;
    partition_end DATE;
    partition_name TEXT;
    i INT;
BEGIN
    -- Verificar si la tabla ya esta particionada
    SELECT EXISTS (
        SELECT 1 FROM pg_partitioned_table
        WHERE partrelid = 'telemetry_readings'::regclass
    ) INTO is_partitioned;

    IF is_partitioned THEN
        RAISE NOTICE 'telemetry_readings ya esta particionada. Saltando.';
        RETURN;
    END IF;

    RAISE NOTICE 'Convirtiendo telemetry_readings a tabla particionada...';

    -- 1. Renombrar tabla original creada por EF Core
    ALTER TABLE telemetry_readings RENAME TO telemetry_readings_old;

    -- 2. Crear tabla particionada con el mismo schema
    CREATE TABLE telemetry_readings (
        snapshot_id         UUID DEFAULT gen_random_uuid(),
        equipment_id        UUID NOT NULL,
        recorded_at         TIMESTAMPTZ NOT NULL,
        location            geography(point, 4326) NOT NULL,
        altitude            DECIMAL(8,2),
        heading             DECIMAL(5,2),
        speed_kmh           DECIMAL(6,2),
        hdop                DECIMAL(4,2),
        satellites          INT,
        engine_hours        DECIMAL(10,2),
        pto_hours           DECIMAL(10,2),
        idle_hours          DECIMAL(10,2),
        cumulative_idle_hours DECIMAL(10,2),
        cumulative_idle_non_operating_hours DECIMAL(10,2),
        distance            DECIMAL(10,2),
        fuel_used           DECIMAL(10,2),
        fuel_level          DECIMAL(5,2),
        def_used            DECIMAL(10,2),
        def_level           DECIMAL(5,2),
        engine_status       INT,
        load_factor         DECIMAL(5,2),
        active_switches     JSONB,
        impact_g_x          DECIMAL(5,2),
        impact_g_y          DECIMAL(5,2),
        impact_g_z          DECIMAL(5,2),
        tilt_lateral        DECIMAL(5,2),
        tilt_longitudinal   DECIMAL(5,2),
        hydraulic_pressure  DECIMAL(10,2),
        engine_temperature  DECIMAL(5,2),
        ambient_temperature DECIMAL(5,2),
        PRIMARY KEY (snapshot_id, recorded_at)
    ) PARTITION BY RANGE (recorded_at);

    -- 3. Crear particiones mensuales (mes actual + 12 meses adelante)
    FOR i IN 0..12 LOOP
        partition_start := date_trunc('month', CURRENT_DATE) + (i || ' months')::interval;
        partition_end := partition_start + '1 month'::interval;
        partition_name := 'telemetry_readings_' || to_char(partition_start, 'YYYY_MM');

        EXECUTE format(
            'CREATE TABLE IF NOT EXISTS %I PARTITION OF telemetry_readings
             FOR VALUES FROM (%L) TO (%L)',
            partition_name, partition_start, partition_end
        );

        RAISE NOTICE 'Particion creada: %', partition_name;
    END LOOP;

    -- 4. Migrar datos existentes (si hay)
    INSERT INTO telemetry_readings SELECT * FROM telemetry_readings_old;

    -- 5. Recrear indices
    CREATE INDEX idx_telemetry_equipment_time
        ON telemetry_readings (equipment_id, recorded_at DESC);

    -- 6. Recrear FK
    ALTER TABLE telemetry_readings
        ADD CONSTRAINT fk_telemetry_equipment
        FOREIGN KEY (equipment_id) REFERENCES equipment(equipment_id);

    -- 7. Drop tabla vieja
    DROP TABLE telemetry_readings_old;

    RAISE NOTICE 'telemetry_readings convertida a particionada exitosamente.';
END $$;
