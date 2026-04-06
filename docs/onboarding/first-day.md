# Guia de Primer Dia

Bienvenido al equipo de RentMaq Pro. Esta guia te llevara desde cero hasta un entorno funcional con contexto suficiente para hacer tu primera contribucion.

---

## 1. Leer la documentacion base

Dedica las primeras 2-3 horas a leer estos documentos en orden. No escribas codigo hasta completar la lectura.

| Orden | Documento | Tiempo estimado | Que obtendras |
|-------|-----------|----------------|---------------|
| 1 | `README.md` | 15 min | Vision general, estructura, reglas criticas |
| 2 | `CLAUDE.md` (o `.cursorrules` si usas Cursor) | 30 min | Las 4 reglas inquebrantables, los 7 workers, state machines |
| 3 | `docs/ADR-001-spec.md` Secciones 1-6 | 60 min | Esquema de BD completo, logica de cada worker, normativas |
| 4 | `docs/onboarding/architecture-overview.md` | 20 min | Por que el sistema esta disenado asi |
| 5 | `docs/architecture/state-machines.md` | 15 min | Diagramas de las 4 maquinas de estado |

---

## 2. Levantar el entorno local

```bash
# Clonar repositorio
git clone https://github.com/[org]/rentmaq-pro.git
cd rentmaq-pro

# Configurar variables de entorno
cp .env.example .env
# Revisar y ajustar valores en .env si es necesario

# Levantar infraestructura
docker compose up -d

# Verificar que PostgreSQL + PostGIS esten corriendo
docker compose ps

# Aplicar migraciones
dotnet ef database update --project src/RentMaq.Infrastructure

# Ejecutar la API (incluye los 7 workers)
dotnet run --project src/RentMaq.API

# Verificar
curl http://localhost:5000/health
```

---

## 3. Ejecutar los tests

```bash
dotnet test src/RentMaq.Tests/
```

Todos los tests deben pasar en verde antes de continuar. Si alguno falla, reportar al tech lead antes de asumir que es un problema de tu entorno.

---

## 4. Conocer tu asignacion

1. Tu tech lead te asignara un worker o feature especifico.
2. Revisa el Conductor Track correspondiente en `conductor/tracks/[nombre-del-worker]/`.
3. Lee el `spec.md` del track para entender la especificacion.
4. Lee el `plan.md` para ver las tareas y sub-tareas planificadas.

---

## 5. Hacer tu primer PR

1. Crear una rama desde `main` con el formato `feature/[track-name]/[descripcion-breve]`.
2. Implementar siguiendo el plan del Conductor Track.
3. Antes de hacer push, validar la No-Join Rule:

```bash
grep -r "current_status" src/RentMaq.Workers/Financial/
grep -r "rental_contracts" src/RentMaq.Workers/Operational/
# Ambos deben retornar cero resultados
```

4. Ejecutar `dotnet test` y verificar que todo pase.
5. Crear el PR con referencia al track de Conductor.

---

## Preguntas frecuentes del primer dia

**P: Por que hay dos state machines separadas?**
R: Para que un equipo reparado pueda rentarse a otro cliente aunque el contrato anterior siga en disputa. Ver `docs/onboarding/architecture-overview.md`.

**P: Puedo consultar `equipment.current_status` desde un worker financiero?**
R: No. Nunca. Ver No-Join Rule en `CLAUDE.md` Seccion 1.

**P: Que pasa si un test de integracion tarda mucho?**
R: Los tests que involucran PostGIS pueden ser mas lentos. Verificar que Docker este corriendo y que el contenedor de BD este healthy.

---

Ultima actualizacion: 2026-04-06. Responsable: Onboarding / Tech Lead. Estado: Vigente.
