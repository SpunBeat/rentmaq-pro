-- =============================================================================
-- RentMaq Pro — Indices espaciales GiST
-- No soportados nativamente por EF Core Fluent API.
-- Ejecutar DESPUES de dotnet ef database update y 001-post-migration-partitioning.sql.
-- Idempotente: usa IF NOT EXISTS.
-- =============================================================================

-- Indice GiST en geofences.perimeter
-- Usado por Worker 1 (TelemetryIngestionWorker) para ST_Covers(perimeter, location)
CREATE INDEX IF NOT EXISTS idx_geofences_perimeter
    ON geofences USING GIST (perimeter);

-- Indice GiST en telemetry_readings.location
-- Optimiza queries espaciales sobre posiciones GPS de equipos
CREATE INDEX IF NOT EXISTS idx_telemetry_location
    ON telemetry_readings USING GIST (location);
