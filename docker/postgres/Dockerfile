# =============================================================================
# RentMaq Pro — PostgreSQL 16 + PostGIS 3 (Multi-arch)
#
# La imagen oficial postgis/postgis:16-3.4 NO publica variante ARM64.
# Esta imagen extiende postgres:16-bookworm (multi-arch: amd64 + arm64)
# e instala PostGIS desde los repositorios de Debian.
#
# Resultado: imagen funcional en MacBook Apple Silicon y Linux AMD64
# sin emulacion QEMU.
# =============================================================================
FROM postgres:16-bookworm

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        postgresql-16-postgis-3 \
        postgresql-16-postgis-3-scripts \
    && rm -rf /var/lib/apt/lists/*
