# Arquitectura -- Indice de Decisiones

Este directorio contiene los Architecture Decision Records (ADRs), diagramas de estado y documentacion de interaccion entre componentes del sistema RentMaq Pro.

---

## ADRs

| ID | Titulo | Estado | Fecha | Autor |
|----|--------|--------|-------|-------|
| ADR-001 | Esquema de Base de Datos y Workers de Orquestacion para Telemetria, Mantenimiento y Facturacion | Propuesto | 2026-04-06 | Noe (Director de Arquitectura) |

### Estados posibles

- **Propuesto:** En revision por el equipo. Puede cambiar antes de ser aceptado.
- **Aceptado:** Decision vigente. El codigo debe cumplir con lo documentado.
- **Deprecado:** Reemplazado por un ADR posterior. Se conserva como referencia historica.

---

## Documentos de Arquitectura

| Documento | Descripcion |
|-----------|-------------|
| [state-machines.md](state-machines.md) | Diagramas Mermaid y tablas de transicion de las 4 state machines |
| [worker-interactions.md](worker-interactions.md) | Diagrama de interaccion entre los 7 workers y las tablas de BD |

---

Ultima actualizacion: 2026-04-06. Responsable: Arquitectura. Estado: Vigente.
