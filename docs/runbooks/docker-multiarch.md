# Runbook: Docker Multi-Arch en RentMaq Pro

## Contexto

El equipo de desarrollo trabaja con hardware mixto:

| Entorno | Arquitectura | Ejemplo |
|---------|-------------|---------|
| Desarrollo local | ARM64 (aarch64) | MacBook con Apple Silicon (M1/M2/M3/M4) |
| CI/CD | AMD64 (x86_64) | GitHub Actions runners |
| Staging / Produccion | AMD64 (x86_64) | Linux en Azure o GCP |

El `compose.yml` de RentMaq Pro funciona en todas las arquitecturas **sin modificaciones manuales**. Este runbook explica por que y como solucionar problemas.

---

## Por que funciona multi-arch

Las imagenes Docker que usa el proyecto publican **manifiestos multi-arch** en Docker Hub / MCR. Un manifiesto multi-arch es un indice que apunta a multiples variantes de la misma imagen, una por arquitectura.

Cuando ejecutas `docker pull postgis/postgis:16-3.4`:
- En un Mac ARM, Docker descarga la variante `linux/arm64`.
- En un runner AMD64, Docker descarga la variante `linux/amd64`.

No se necesita `--platform` en el `compose.yml` ni en los `Dockerfile`. Docker resuelve la arquitectura del host automaticamente.

### Imagenes usadas y soporte multi-arch

| Imagen | Tag | amd64 | arm64 | Notas |
|--------|-----|-------|-------|-------|
| postgis/postgis | 16-3.4 | Si | **No** | No publica ARM64. Usamos Dockerfile custom (`docker/postgres/Dockerfile`) basado en `postgres:16-bookworm` + paquete `postgresql-16-postgis-3` |
| postgres | 16-bookworm | Si | Si | Imagen base para nuestro Dockerfile custom de PostGIS |
| redis | 7-alpine | Si | Si | Docker Hub |
| mcr.microsoft.com/dotnet/sdk | 8.0 | Si | Si | Microsoft Container Registry |
| mcr.microsoft.com/dotnet/aspnet | 8.0 | Si | Si | Microsoft Container Registry |
| node | 20-alpine | Si | Si | Docker Hub |
| nginx | alpine | Si | Si | Docker Hub |

---

## Troubleshooting

### Problema: WARNING platform mismatch

```
WARNING: The requested image's platform (linux/amd64) does not match
the detected host platform (linux/arm64/v8)
```

**Causa:** La imagen que estas usando NO tiene variante ARM64. Docker la ejecuta bajo emulacion QEMU, lo cual es lento y puede fallar.

**Solucion:**
1. Verificar en Docker Hub que el tag tenga la columna `OS/ARCH` con `linux/arm64`.
2. Si no la tiene, buscar una imagen alternativa que si la tenga.
3. Como ultimo recurso, crear un Dockerfile que compile desde fuente en la arquitectura del host.

### Problema: Rendimiento lento en Mac ARM

**Causa:** La imagen esta corriendo bajo emulacion QEMU en lugar de nativamente.

**Verificacion:**
```bash
# Ver la arquitectura real del contenedor
docker exec rentmaq-db uname -m
# Esperado en Mac ARM: aarch64
# Si retorna x86_64, esta emulando
```

**Solucion:** Usar una imagen que publique variante ARM64 nativa. Todas las imagenes del `compose.yml` de RentMaq Pro ya cumplen este criterio.

### Problema: pg_isready timeout al iniciar

**Causa:** PostGIS tarda mas que PostgreSQL vanilla en inicializar, especialmente la primera vez que crea la base de datos con extensiones espaciales. En ARM bajo emulacion, puede tardar aun mas.

**Solucion:** El `compose.yml` ya tiene ajustes para esto:
```yaml
healthcheck:
  interval: 5s
  timeout: 5s
  retries: 15
  start_period: 10s
```

Si persiste, aumentar `retries` a 20 o `start_period` a 20s.

### Problema: Error de compilacion en Dockerfile de API

**Causa potencial:** Alguna dependencia NuGet tiene binarios nativos compilados solo para una arquitectura.

**Solucion:** .NET 8 con JIT (modo por defecto) es cross-platform. El `dotnet publish` en el Dockerfile NO especifica `-r <RID>` intencionalmente. Si necesitas AOT (ahead-of-time compilation), tendras que especificar el RID y potencialmente usar `docker buildx` con builders multi-plataforma.

---

## Comandos de verificacion

```bash
# --- Verificar que la imagen es nativa (no emulada) ---
docker inspect postgis/postgis:16-3.4 --format '{{.Architecture}}'
# Esperado en Mac ARM: arm64
# Esperado en Linux AMD: amd64

# --- Verificar que PostGIS esta habilitado ---
docker exec rentmaq-db psql -U rentmaq -d rentmaq_db -c "SELECT PostGIS_Version();"
# Esperado: 3.4 ...

# --- Verificar arquitectura del contenedor en ejecucion ---
docker exec rentmaq-db uname -m
# Esperado: aarch64 (ARM) o x86_64 (AMD)

# --- Verificar que los ENUMs se crearon correctamente ---
docker exec rentmaq-db psql -U rentmaq -d rentmaq_db -c \
  "SELECT typname FROM pg_type WHERE typtype = 'e' ORDER BY typname;"
# Esperado: 9 ENUMs (alert_type_enum, billing_model_enum, ...)

# --- Verificar Redis ---
docker exec rentmaq-redis redis-cli ping
# Esperado: PONG

# --- Verificar que todos los servicios estan healthy ---
docker compose ps
# Todos deben mostrar (healthy)
```

---

## Regla para agregar nuevas imagenes al stack

Antes de agregar una nueva imagen Docker al `compose.yml`:

1. Ir a la pagina del tag en Docker Hub.
2. Verificar que en la seccion `OS/ARCH` aparezca `linux/arm64` ademas de `linux/amd64`.
3. Si **no** tiene `linux/arm64`: NO la uses. Buscar alternativa o crear un Dockerfile que compile desde fuente.
4. Probar en Mac ARM antes de hacer merge. Una imagen que funciona en CI (AMD64) puede fallar o ser extremadamente lenta en ARM si no tiene variante nativa.

---

## Referencia

- Docker multi-platform builds: `docker buildx build --platform linux/amd64,linux/arm64`
- Compose spec: `platform` es opcional y solo debe usarse cuando se necesita forzar una arquitectura especifica (raro).
- `.env.example` en la raiz del proyecto para variables de entorno.

---

Ultima actualizacion: 2026-04-07. Responsable: DevOps. Estado: Vigente.
