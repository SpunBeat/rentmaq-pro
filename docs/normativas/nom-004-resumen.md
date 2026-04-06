# NOM-004-STPS-1999 -- Resumen Ejecutivo

## Proposito

Norma Oficial Mexicana que establece las condiciones de seguridad para maquinaria y equipo que se utilice en los centros de trabajo. Aplica a todas las operaciones de mantenimiento de RentMaq Pro.

---

## Articulos con Impacto Directo en el Sistema

### Art. 7.2.2 -- Procedimientos de bloqueo de energia (LOTO)

La norma exige que al realizar mantenimiento en maquinaria:

**a) Protectores y dispositivos de seguridad:** Deben reinstalarse antes de que el equipo vuelva a operacion. El sistema verifica esto con el campo `maintenance_work_orders.protectors_reinstalled`.

**c) Bloqueo de energia (Lockout/Tagout):** Antes de intervenir el equipo, se deben bloquear todas las fuentes de energia (electrica, hidraulica, neumatica, mecanica) y colocar candados y tarjetas de aviso. El sistema verifica esto con el campo `maintenance_work_orders.loto_applied`.

**Implementacion en el sistema:**

| Campo en BD | Tabla | Validacion |
|-------------|-------|------------|
| `loto_applied` | `maintenance_work_orders` | Debe ser `TRUE` para completar orden |
| `loto_applied_at` | `maintenance_work_orders` | Timestamp del bloqueo |
| `loto_timeout_hours` | `maintenance_work_orders` | Genera alerta si se excede (default 24h) |
| `protectors_reinstalled` | `maintenance_work_orders` | Debe ser `TRUE` para completar orden |
| `protectors_verified_at` | `maintenance_work_orders` | Timestamp de la verificacion |

**Regla del sistema:** La transicion `IN_MAINTENANCE -> AVAILABLE` requiere ambas condiciones en `TRUE`. No existe bypass. El Worker 3 (MaintenanceOrchestrationWorker) no ejecuta la transicion si alguna es `FALSE`. Ver `CLAUDE.md` Seccion 3.

---

### Art. 7.2.3 -- Registro y bitacora de mantenimiento

La norma exige conservar registros de todas las actividades de mantenimiento durante un periodo minimo de 12 meses.

**Implementacion en el sistema:**

- La tabla `maintenance_work_orders` funciona como bitacora legal auditable.
- Cada orden registra: fecha de ejecucion, tecnico responsable, tipo de trabajo, piezas utilizadas, horas de labor, notas del tecnico.
- El Worker 6 (ComplianceDocumentWorker) genera PDFs de la bitacora NOM-004 para cada orden completada.
- Las ordenes no se eliminan fisicamente de la base de datos. La politica de retencion es de minimo 12 meses.

---

### Implicaciones para certificaciones de operadores

La NOM-004 requiere que los operadores de maquinaria esten capacitados en el uso del equipo especifico que operan.

**Implementacion en el sistema:**

- La tabla `operator_certifications` registra las constancias DC-3 de cada operador.
- El campo `equipment_type_certified` indica para que tipo de maquinaria esta certificado.
- La transicion `RESERVED -> ACTIVE` del contrato verifica que exista una constancia DC-3 vigente para el operador designado.

---

## Documentos generados por el sistema

El Worker 6 genera los siguientes documentos de cumplimiento:

1. **Bitacora NOM-004:** Registro completo de la intervencion de mantenimiento.
2. **Protocolo LOTO:** Detalle del bloqueo de energia aplicado y su retiro.
3. **Certificado de protectores:** Verificacion de reinstalacion de dispositivos de seguridad.
4. **Constancias DC-3:** Registro de capacitacion del operador.

---

## Referencia cruzada

- `CLAUDE.md` Seccion 3 (LOTO Gatekeeper).
- `docs/ADR-001-spec.md` Seccion 3.3 (tablas de mantenimiento).
- `docs/runbooks/loto-timeout.md` (procedimiento operativo por timeout).

---

Ultima actualizacion: 2026-04-06. Responsable: Compliance / Operaciones. Estado: Vigente.
