# RentMaq Pro

Sistema enterprise de renta de maquinaria ligera con telemetria ISO 15143-3, cumplimiento NOM-004-STPS y facturacion CFDI 4.0.

**Stack:** .NET Core 8 | PostgreSQL 16 + PostGIS 3.4 | Redis 7 | Angular 18 + Material | Google Conductor + Antigravity

---

## Vision General

RentMaq Pro gestiona el ciclo de vida completo de maquinaria ligera en renta: miniexcavadoras, minicargadores, plataformas de elevacion, compactadores y generadores. El sistema ingesta telemetria en tiempo real via el estandar AEMP 2.0 (GPS, horometros, codigos de falla J1939) y la evalua contra umbrales configurables por tipo y modelo de equipo. Orquesta mantenimiento preventivo y correctivo con cumplimiento estricto de NOM-004-STPS-1999, incluyendo protocolo LOTO, reinstalacion de protectores y bitacoras auditables con retencion de 12 meses. Factura electronicamente via CFDI 4.0 operando tres flujos fiscales mutuamente excluyentes: cargo por dano, sustitucion por error y bonificacion. El nucleo arquitectonico son dos maquinas de estado concurrentes e independientes — el estado fisico del activo y el estado financiero del contrato — comunicadas exclusivamente mediante eventos y tablas intermedias, nunca por JOINs directos.

---

## Inicio Rapido

### Prerequisitos

- .NET SDK 8.0+
- PostgreSQL 16 con extension PostGIS 3.4
- Node.js 20 LTS + Angular CLI 18
- Docker y Docker Compose V2 (para infraestructura local)
- Gemini CLI con extension Conductor (opcional, para flujo de desarrollo asistido)

> **Multi-arch:** El entorno Docker funciona sin modificaciones en MacBook Apple Silicon (ARM64), GitHub Actions (AMD64) y cloud Linux (AMD64). La imagen de PostGIS usa un Dockerfile custom basado en `postgres:16-bookworm` porque la oficial `postgis/postgis:16-3.4` no publica variante ARM64. Ver `docs/runbooks/docker-multiarch.md`.

### Pasos

```bash
# 1. Clonar repositorio
git clone https://github.com/[org]/rentmaq-pro.git
cd rentmaq-pro

# 2. Configurar variables de entorno
cp .env.example .env
# Editar .env con credenciales locales

# 3. Levantar infraestructura (PostgreSQL + PostGIS + Redis)
docker compose --profile infra up -d

# 4. Aplicar migraciones de base de datos
dotnet ef database update --project src/RentMaq.Infrastructure --startup-project src/RentMaq.API

# 5. Ejecutar backend (API + Workers)
dotnet run --project src/RentMaq.API

# 6. Instalar dependencias frontend
cd src/RentMaq.Web
npm install

# 7. Ejecutar frontend
ng serve

# 8. Verificar que todo funcione
curl http://localhost:5000/health
```

> **Nota:** Los 7 workers se levantan automaticamente con el host de la API. Todos implementan `IHostedService` y se registran en el contenedor de dependencias al iniciar la aplicacion. No requieren procesos separados.

> **Stack completo en Docker:** Si prefieres correr todo en contenedores (sin `dotnet run` local), usa `docker compose --profile dev up -d`. Esto levanta BD + Redis + API + Web en un solo comando.

---

## Swagger (OpenAPI)

La API expone documentacion interactiva con [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) (`AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI`). El host registra tambien `AddEndpointsApiExplorer()` para que los controladores convencionales aparezcan en el documento OpenAPI.

### Cuando esta activo

El middleware de Swagger **solo se habilita en entorno `Development`**. En `Program.cs`, `UseSwagger()` y `UseSwaggerUI()` estan dentro de `if (app.Environment.IsDevelopment())`, asi que en Staging/Produccion no se publica la UI ni el endpoint JSON por defecto (evita exponer la superficie de la API en internet sin una decision explicita).

### Como abrirlo

| Como ejecutas la API | URL tipica de la UI |
|----------------------|---------------------|
| `dotnet run --project src/RentMaq.API` (perfil **https** en `launchSettings.json`) | `https://localhost:7236/swagger` |
| Mismo comando, perfil **http** | `http://localhost:5250/swagger` |
| `docker compose --profile dev` (mapeo `5000:8080` y `ASPNETCORE_ENVIRONMENT=Development` por defecto) | `http://localhost:5000/swagger` |

La especificacion OpenAPI en JSON suele estar en `/swagger/v1/swagger.json` (nombre de documento por defecto `v1`). Swagger UI la consume para listar operaciones, esquemas y el boton **Try it out** para invocar endpoints desde el navegador.

### Notas de uso

- Los endpoints que requieran autenticacion mostraran la operacion en Swagger, pero las pruebas desde la UI fallaran hasta que configures cabeceras (por ejemplo `Authorization`) si el proyecto las define mas adelante.
- Si cambias puertos (`applicationUrl`, mapeos de Docker o variables `ASPNETCORE_HTTP_PORTS`), ajusta la URL base; la ruta `/swagger` se mantiene.

---

## Estructura del Proyecto

```
rentmaq-pro/
├── .cursorrules                    # Reglas de arquitectura para Cursor AI
├── .env.example                    # Plantilla de variables de entorno
├── CLAUDE.md                       # Reglas de arquitectura para Claude Code / Conductor
├── README.md
├── compose.yml                     # Docker Compose V2 (profiles: infra, dev, ci)
├── docker/
│   └── postgres/
│       └── Dockerfile              # postgres:16-bookworm + PostGIS 3 (multi-arch)
├── conductor/                      # Google Conductor (desarrollo asistido)
│   ├── product.md                  # Vision del producto
│   ├── product-guidelines.md       # Reglas de negocio y desacoplamiento
│   ├── tech-stack.md
│   ├── workflow.md
│   └── tracks/                     # Un track por feature/worker
│       ├── telemetry-ingestion-worker/
│       ├── alert-evaluation-worker/
│       ├── maintenance-orchestration-worker/
│       ├── financial-impact-worker/
│       ├── cfdi-lifecycle-worker/
│       ├── compliance-document-worker/
│       └── contract-lifecycle-worker/
├── docs/                           # Documentacion viva del proyecto
│   ├── ADR-001-spec.md             # Esquema de BD + Workers (decision vigente)
│   ├── architecture/               # Diagramas C4, flujos, state machines
│   ├── runbooks/                   # Procedimientos operativos
│   │   ├── loto-timeout.md         # Timeout LOTO: alerta, no desbloqueo
│   │   ├── geofence-breach.md      # Evaluacion de falsos positivos GPS
│   │   ├── fiscal-damage-charge.md # Flujo cargo por dano -> CFDI
│   │   └── docker-multiarch.md     # Troubleshooting multi-arch (ARM/AMD)
│   ├── onboarding/                 # Guias para nuevos desarrolladores
│   └── normativas/                 # Referencias NOM-004, NOM-009, CFDI 4.0
├── src/
│   ├── RentMaq.API/                # Host ASP.NET Core (API REST + Workers)
│   │   └── Dockerfile              # Multi-stage: SDK build -> aspnet runtime
│   ├── RentMaq.Domain/             # Entidades, enums, value objects, interfaces
│   │   ├── Equipment/              # Bounded context: Telemetria y Equipos
│   │   ├── Maintenance/            # Bounded context: Mantenimiento y Compliance
│   │   ├── Contracts/              # Bounded context: Contratos y State Machine
│   │   └── Billing/                # Bounded context: Facturacion y CFDI
│   ├── RentMaq.Application/        # Casos de uso, MediatR, DTOs
│   ├── RentMaq.Infrastructure/     # EF Core, PostgreSQL, integracion PAC, AEMP
│   │   ├── Persistence/            # DbContext, Migrations, Configurations
│   │   ├── Telemetry/              # Cliente AEMP 2.0 (polling HTTP)
│   │   └── Fiscal/                 # Cliente PAC (Facturapi, SW Sapiens)
│   ├── RentMaq.Workers/            # Los 7 workers como IHostedService
│   │   ├── Operational/            # Workers 1, 2, 3, 6 (dominio fisico)
│   │   └── Financial/              # Workers 4, 5, 7 (dominio financiero)
│   ├── RentMaq.Web/                # Angular 18 + Material (frontend)
│   │   └── Dockerfile              # node:20-alpine build -> nginx:alpine
│   └── RentMaq.Tests/              # Tests unitarios e integracion
│       ├── Workers/
│       ├── Domain/
│       └── Integration/
└── scripts/
    ├── init-db.sql                 # PostGIS + uuid-ossp + 9 ENUMs (primer arranque)
    ├── seed-data.sql               # Datos de prueba (equipos, tenants, umbrales)
    └── create-partitions.sql       # Crea particiones mensuales de telemetry_readings
```

---

## Reglas de Arquitectura (Resumen Ejecutivo)

El sistema tiene cuatro reglas inquebrantables. Las violaciones son causa de rechazo automatico en code review.

**Regla 1 -- No-Join Rule:** Los workers financieros (4, 5, 7) NUNCA consultan `equipment.current_status`. Los workers operativos (1, 2, 3, 6) NUNCA consultan `rental_contracts.status`. La comunicacion entre dominios se realiza exclusivamente mediante tablas intermedias (`telemetry_alerts`, `damage_assessments`, `extraordinary_charges`) o eventos. Detalle completo en `CLAUDE.md` Seccion 1.

**Regla 2 -- Tres Flujos Fiscales Excluyentes:** Un cargo por dano genera un CFDI de Ingreso nuevo e independiente. NUNCA sustituye ni cancela la factura mensual de renta. La sustitucion (Tipo 04) es exclusiva para correcciones administrativas. La nota de credito (TipoRelacion 01) es exclusiva para bonificaciones por falla del arrendador. Detalle en `CLAUDE.md` Seccion 2.

**Regla 3 -- LOTO Gatekeeper:** Un equipo en `IN_MAINTENANCE` NO puede transicionar a `AVAILABLE` sin dos condiciones verificadas en base de datos: `loto_applied = TRUE` y `protectors_reinstalled = TRUE`. No existe bypass administrativo. No existe desbloqueo por timeout. Si la orden excede `loto_timeout_hours`, se alerta al supervisor pero NO se desbloquea. Detalle en `CLAUDE.md` Seccion 3.

**Regla 4 -- Geofencing = Alerta, NUNCA Trigger:** Cuando el GPS reporta que un equipo salio del poligono autorizado, el sistema inserta una alerta `GEOFENCE_BREACH`. NUNCA transiciona el contrato a `RETURNING`. La transicion requiere siempre accion humana. Detalle en `CLAUDE.md` Seccion 4.

> **Antes de hacer tu primer PR, lee completo el archivo `CLAUDE.md`.** Estas reglas se aplican en code review y las violaciones son causa de rechazo automatico.

---

## Los 7 Workers

| Worker | Dominio | Disparador | SLA | Conductor Track |
|--------|---------|------------|-----|-----------------|
| TelemetryIngestionWorker | Operativo | Cron 5 min | < 5 min flota | `conductor/tracks/telemetry-ingestion-worker/` |
| AlertEvaluationWorker | Operativo | Event: nueva lectura | < 30 seg | `conductor/tracks/alert-evaluation-worker/` |
| MaintenanceOrchestrationWorker | Operativo | Event + Cron horario | < 2 min correctivo | `conductor/tracks/maintenance-orchestration-worker/` |
| ComplianceDocumentWorker | Operativo | Event + Cron diario | Bajo demanda | `conductor/tracks/compliance-document-worker/` |
| FinancialImpactWorker | Financiero | Event: dano aprobado | Bajo demanda | `conductor/tracks/financial-impact-worker/` |
| CfdiLifecycleWorker | Financiero | Event + Cron mensual | < 60 seg timbrado | `conductor/tracks/cfdi-lifecycle-worker/` |
| ContractLifecycleWorker | Financiero | Event + usuario | Bajo demanda | `conductor/tracks/contract-lifecycle-worker/` |

Cada worker implementa `IHostedService`. Su especificacion detallada esta en `docs/ADR-001-spec.md` Seccion 4 y en el Conductor Track correspondiente.

---

## Normativas Aplicables

| Normativa | Impacto en el Sistema | Referencia |
|-----------|----------------------|------------|
| NOM-004-STPS-1999 | LOTO, protectores, bitacoras de mantenimiento (12 meses) | `docs/normativas/nom-004-resumen.md` |
| NOM-009-STPS-2011 | Plataformas de elevacion, certificacion ANSI A92 anual | `docs/normativas/` |
| ISO 15143-3 / AEMP 2.0 | Estandar de telemetria para maquinaria de construccion | `docs/ADR-001-spec.md` Seccion 3.2 |
| J1939 | Protocolo de diagnostico de fallas mecanicas (SPN + FMI) | `docs/ADR-001-spec.md` Seccion 3.2 |
| CFDI 4.0 | Facturacion electronica mexicana, tres flujos excluyentes | `docs/normativas/cfdi-flujos.md` |
| NIF D-5 | Tratamiento contable de depositos en garantia como Pasivo | `docs/ADR-001-spec.md` Seccion 3.4 |

---

## Documentacion Viva

El proyecto utiliza documentacion viva: los documentos en `docs/` evolucionan con el codigo y se mantienen actualizados como parte del flujo de desarrollo. Cada PR que modifica comportamiento documentado debe actualizar el documento correspondiente.

| Directorio | Proposito | Documentos clave |
|------------|-----------|-----------------|
| `docs/architecture/` | ADRs, diagramas C4, state machines, interaccion entre workers | `state-machines.md`, `worker-interactions.md` |
| `docs/runbooks/` | Procedimientos operativos para alertas y flujos criticos | `loto-timeout.md`, `geofence-breach.md`, `fiscal-damage-charge.md`, `docker-multiarch.md` |
| `docs/onboarding/` | Guias para nuevos desarrolladores | `first-day.md`, `architecture-overview.md` |
| `docs/normativas/` | Resumenes ejecutivos de normativas aplicables al sistema | `nom-004-resumen.md`, `cfdi-flujos.md` |

---

## Flujo de Desarrollo con Conductor

El equipo utiliza Google Conductor (Gemini CLI) para desarrollo asistido por IA:

1. **`gemini conductor:setup`** -- Configuracion inicial del proyecto (ya ejecutado).
2. **`gemini conductor:newTrack`** -- Crea un nuevo track para cada feature o worker.
3. El agente genera `spec.md` (especificacion) y `plan.md` (tareas y sub-tareas).
4. **`gemini conductor:implement`** -- El agente ejecuta el plan, pausando en checkpoints para revision humana.
5. **`gemini conductor:review`** -- Revision automatizada contra las guidelines del proyecto (`conductor/product-guidelines.md`).

Cada worker tiene su propio track en `conductor/tracks/`. El track contiene la especificacion, el plan de implementacion y el historial de decisiones del agente.

---

## Como Contribuir

1. Todo PR debe pasar los criterios de aceptacion del ADR-001 (ver Seccion 9 del documento).
2. Los workers operativos y financieros van en carpetas separadas (`src/RentMaq.Workers/Operational/` y `Financial/`).
3. Validar la No-Join Rule antes de hacer push:

```bash
# Debe retornar cero resultados
grep -r "current_status" src/RentMaq.Workers/Financial/
grep -r "rental_contracts" src/RentMaq.Workers/Operational/
```

4. Los tests de integracion deben verificar los gatekeepers:
   - LOTO: un equipo sin `loto_applied = TRUE` no puede salir de `IN_MAINTENANCE`.
   - ANSI A92: certificacion vencida bloquea transicion a `AVAILABLE`.
   - Geofence: una lectura fuera de poligono genera alerta, nunca transicion de contrato.

5. Usar el flujo de Conductor para features nuevas. No iniciar implementacion sin `spec.md` y `plan.md` aprobados.
