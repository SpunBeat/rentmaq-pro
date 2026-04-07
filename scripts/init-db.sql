-- =============================================================================
-- RentMaq Pro — Script de inicializacion de base de datos
-- Ejecutado automaticamente por PostgreSQL al crear la BD por primera vez
-- (montado en /docker-entrypoint-initdb.d/).
--
-- Contenido:
--   1. Extensiones requeridas (PostGIS, uuid-ossp)
--
-- NOTA: Los 9 ENUMs del ADR-001 son gestionados por EF Core via migraciones.
-- NO crearlos aqui para evitar conflictos con dotnet ef database update.
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
-- Verificacion
-- =============================================================================
DO $$
BEGIN
    RAISE NOTICE '=== RentMaq Pro: init-db.sql completado ===';
    RAISE NOTICE 'PostGIS version: %', PostGIS_Version();
END $$;
