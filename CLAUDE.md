# RentMaq Pro — Architecture Guards

Sistema enterprise de renta de maquinaria ligera.
Stack: .NET Core 8, PostgreSQL + PostGIS, Angular Material.
Normativas: ISO 15143-3, J1939, NOM-004-STPS-1999, NOM-009-STPS-2011, CFDI 4.0, NIF D-5.

Documento de referencia completo: `docs/ADR-001-spec.md`

---

## REGLAS INQUEBRANTABLES

Estas reglas tienen prioridad absoluta. Cualquier PR que las viole debe ser rechazado.

### 1. No-Join Rule (Desacoplamiento Físico vs. Financiero)

El sistema opera dos state machines concurrentes e independientes:
- `equipment.current_status` → estado FÍSICO del activo (AVAILABLE, RENTED, IN_MAINTENANCE, DECOMMISSIONED)
- `rental_contracts.status` → estado FINANCIERO del contrato (DRAFT, RESERVED, ACTIVE, SUSPENDED_MAINTENANCE, RETURNING, INSPECTION, OVERDUE, IN_DISPUTE, CLOSED)

**PROHIBIDO:**
- Workers financieros (FinancialImpactWorker, CfdiLifecycleWorker, ContractLifecycleWorker) NO deben hacer SELECT, JOIN ni inyectar repositorios que consulten `equipment.current_status`.
- Workers operativos (AlertEvaluationWorker, MaintenanceOrchestrationWorker, ComplianceDocumentWorker) NO deben hacer SELECT, JOIN ni inyectar repositorios que consulten `rental_contracts.status`.
- La comunicación entre dominios se realiza EXCLUSIVAMENTE mediante inserción de registros en tablas intermedias (`telemetry_alerts`, `damage_assessments`, `extraordinary_charges`) o eventos en el message bus.

**POR QUÉ:** Un equipo reparado (`AVAILABLE`) debe poder rentarse a un nuevo cliente aunque el contrato anterior siga en `IN_DISPUTE`. Acoplar las state machines bloquea la monetización del activo por lentitud de cobranza.

**VERIFICACIÓN:** `grep -r "current_status" src/Workers/Financial/` debe retornar cero resultados. `grep -r "rental_contracts" src/Workers/Operational/` debe retornar cero resultados (excepto tablas intermedias).

### 2. Prohibición Fiscal: Tres Flujos Mutuamente Excluyentes

Los cargos por daños a la maquinaria NUNCA sustituyen, modifican ni cancelan la factura de renta original. La factura de arrendamiento y la penalización por mal uso son eventos fiscalmente independientes.

**Flujo 1 — Cargo por Daño (negligencia del cliente):**
- Se emite un CFDI de Ingreso (`cfdi_type = 'I'`) NUEVO e independiente.
- Se deduce del depósito en garantía (migra de Pasivo a Ingreso).
- La factura mensual de renta NO se toca, NO se cancela, NO se sustituye.

**Flujo 2 — Sustitución por Error (RFC incorrecto, monto equivocado):**
- Se cancela el CFDI original con `cancel_reason = '01'`.
- Se reexpide con `relation_type = '04'`.
- SOLO para correcciones administrativas. NUNCA para cargos por daño.

**Flujo 3 — Bonificación (falla imputable al arrendador):**
- Se emite CFDI de Egreso con `relation_type = '01'` (Nota de Crédito).
- NUNCA se usa para anular transacciones completas (duplicaría el ingreso gravable ante el SAT).

**Si estás escribiendo código que toca `cfdi_documents`:** pregúntate a cuál de los tres flujos pertenece. Si no encaja limpiamente en uno solo, el diseño está mal.

### 3. LOTO Gatekeeper (NOM-004-STPS-1999)

Un equipo en `IN_MAINTENANCE` NO puede transicionar a `AVAILABLE` sin cumplir DOS condiciones en base de datos:
1. `maintenance_work_orders.loto_applied = TRUE` (bloqueo de energía certificado)
2. `maintenance_work_orders.protectors_reinstalled = TRUE` (protectores de seguridad reinstalados)

**PROHIBIDO:**
- Crear endpoints, comandos o bypass administrativos que salten estas validaciones.
- Desbloquear automáticamente por timeout. Si la orden excede `loto_timeout_hours`, se ALERTA al supervisor pero NO se desbloquea.

**POR QUÉ:** Cumplimiento NOM-004 Art. 7.2.2. Omitir LOTO pone en riesgo la vida del siguiente operador. Esto no es un tema de UX — es un tema de responsabilidad penal.

### 4. Geofencing = Alerta, NUNCA Trigger de Transición

Cuando el GPS reporta que un equipo salió del polígono de la obra (`geofences.perimeter`), el sistema:
- INSERTA una alerta `GEOFENCE_BREACH` en `telemetry_alerts`.
- NO transiciona `rental_contracts.status` a `RETURNING`.

La transición a `RETURNING` requiere SIEMPRE acción humana intencionada (cliente en Portal o chofer en App). Única excepción: recolección forzada por mora a día 60.

**POR QUÉ:** GPS industrial tiene precisión CEP95 ~6.2m. Los valores HDOP/PDOP en zonas de construcción generan falsos positivos que provocarían transiciones contractuales erróneas.

---

## LOS 7 WORKERS

Cada worker es un .NET `IHostedService`. Cada uno tiene su Conductor Track en `conductor/tracks/`.

### Dominio Operativo (opera sobre `equipment.current_status`)

**Worker 1 — TelemetryIngestionWorker**
- Cron cada 5 min. Pollea endpoints AEMP 2.0 de cada equipo.
- Escribe `telemetry_readings` (particionada por mes), `telemetry_alerts` (solo GEOFENCE_BREACH).
- SLA: toda la flota en < 5 min.

**Worker 2 — AlertEvaluationWorker**
- Event-driven: nueva lectura en `telemetry_readings`.
- Compara contra `equipment_thresholds` (configurables por tipo/modelo).
- Aplica perfiles de vibración base (`equipment_load_profiles`) para filtrar falsos positivos en compactadores.
- CRITICAL/SHUTDOWN → publica evento para Worker 3.
- SLA: < 30 seg por lectura.

**Worker 3 — MaintenanceOrchestrationWorker**
- Reactivo: alertas CRITICAL/SHUTDOWN.
- Preventivo: cron horario contra `maintenance_schedules`.
- Disparador DUAL: por horómetro (`next_due_hours`) O por calendario (`next_due_date`), lo que ocurra primero.
- Transiciona `equipment.current_status` → `IN_MAINTENANCE`.
- Publica evento (Worker 7 lo consume para `SUSPENDED_MAINTENANCE` en el contrato).
- SLA correctivo: < 2 min. SLA preventivo: < 10 min toda la flota.

**Worker 6 — ComplianceDocumentWorker**
- Event-driven: orden completada. Cron diario: vencimiento de certificaciones.
- Genera PDFs: bitácora NOM-004 (retención 12 meses), protocolo LOTO, certificado protectores, constancias DC-3.
- Timeout LOTO: si `LOTO_APPLIED` > `loto_timeout_hours`, alerta al supervisor. NO desbloquea.
- Alerta 30 días antes de vencimiento ANSI A92 en plataformas.

### Dominio Financiero (opera sobre `rental_contracts.status`)

**Worker 4 — FinancialImpactWorker**
- Event-driven: `damage_assessments.status = 'APPROVED_FOR_CHARGE'`.
- Si daño > depósito disponible: `amount_from_deposit = saldo`, `amount_direct_bill = excedente`.
- Migra depósito: `accounting_classification` → `RECOGNIZED_INCOME`.
- Publica evento para Worker 5 (CFDI de Ingreso NUEVO). NUNCA toca factura mensual.

**Worker 5 — CfdiLifecycleWorker**
- Emite CFDIs (Ingreso renta, Ingreso daño, Egreso bonificación, Pago complemento).
- Integra con PAC (Facturapi, SW Sapiens, etc.).
- Monitorea `cancellation_status` para cancelaciones asíncronas del SAT.
- SLA timbrado: < 60 seg con reintentos automáticos.

**Worker 7 — ContractLifecycleWorker**
- State machine de 9 estados. Event-driven + acciones de usuario.
- `RESERVED → ACTIVE` requiere: depósito recibido + checklist pre-entrega + DC-3 operador.
- `RETURNING` requiere acción humana (excepto día 60 de mora).
- `INSPECTION → CLOSED` solo si checklist post-devolución sin hallazgos.
- `OVERDUE` bloquea CLOSED pero permite transiciones operativas.
- Escalación: día 30 notificación, día 60 recolección forzada, día 90 cobranza jurídica.

---

## STATE MACHINES

### equipment.current_status
```
AVAILABLE → RENTED → IN_MAINTENANCE → AVAILABLE (con LOTO gate)
AVAILABLE → DECOMMISSIONED
IN_MAINTENANCE → DECOMMISSIONED
```
Bloqueo: certificación ANSI A92 expirada impide `IN_MAINTENANCE → AVAILABLE`.

### rental_contracts.status
```
DRAFT → RESERVED → ACTIVE
ACTIVE → SUSPENDED_MAINTENANCE → ACTIVE
ACTIVE → RETURNING → INSPECTION → CLOSED | IN_DISPUTE
ACTIVE → OVERDUE (coexiste con operativas)
IN_DISPUTE → CLOSED (cargo pagado)
OVERDUE → CLOSED (deuda saldada)
```

### deposits.status
```
PENDING_COLLECTION → HELD_AS_LIABILITY (Pasivo, sin IVA/ISR)
HELD_AS_LIABILITY → PARTIALLY_APPLIED | FULLY_APPLIED (migra a Ingreso)
PARTIALLY_APPLIED → FULLY_APPLIED | REFUNDED
HELD_AS_LIABILITY → REFUNDED
```
Constraint: `applied_amount + refunded_amount <= amount` (CHECK en DB).

### extraordinary_charges.status
```
DETECTED → ATTRIBUTED → APPLIED_TO_DEPOSIT → INVOICED
```

---

## ESQUEMA DE BASE DE DATOS

18 tablas en PostgreSQL + PostGIS. Esquema completo en `docs/ADR-001-spec.md` Sección 3.

Reglas críticas de la BD:
- `telemetry_readings` particionada por `RANGE (recorded_at)` con particiones mensuales.
- `telemetry_alerts.spn` y `fmi` son NULLABLE (solo aplican cuando `alert_type = 'FAULT_CODE'`).
- `equipment_thresholds` configura umbrales por tipo/modelo. NUNCA hardcodear umbrales en lógica de workers.
- `equipment_load_profiles` se vincula por make/model/application, NO por equipo individual.
- `damage_assessments.attribution` usa enum de 5 valores (NORMAL_WEAR, TENANT_ATTRIBUTABLE, ENVIRONMENTAL, PRE_EXISTING, UNDER_DISPUTE). NO usar booleano `is_chargeable`.
- `geofences.perimeter` usa `GEOGRAPHY(POLYGON, 4326)` para distancias reales sobre curvatura terrestre.

---

## NORMATIVAS

- **NOM-004-STPS-1999:** Registros de mantenimiento (retención 12 meses), LOTO obligatorio, reinstalación de protectores.
- **NOM-009-STPS-2011:** Plataformas de elevación requieren certificación anual ANSI A92. Certificación vencida = equipo bloqueado.
- **ISO 15143-3 (AEMP 2.0):** Estándar de telemetría. Campos obligatorios: location, engine_hours, fuel_used, fault_codes.
- **J1939:** Protocolo de diagnóstico. Códigos SPN+FMI para fallas mecánicas.
- **CFDI 4.0:** Tres flujos excluyentes. PPD para rentas. Complementos de pago obligatorios.
- **NIF D-5 / Resolución 42/IVA/2019-RF:** Depósitos como Pasivo hasta aplicación. No causan IVA/ISR hasta migrar a Ingreso.

---

## CONVENCIONES DE CÓDIGO

- Nombres de workers: `PascalCase` + sufijo `Worker` (ej. `TelemetryIngestionWorker`).
- Cada worker implementa `IHostedService`.
- UUIDs como PKs en todas las tablas (`gen_random_uuid()`).
- Timestamps siempre en `TIMESTAMPTZ` (UTC).
- JSONB para campos semi-estructurados (checklist items, parts_used, photos).
- ENUMs definidos a nivel de PostgreSQL, NO como strings mágicos en código.
- Toda operación debe ser idempotente. Re-procesar un evento no genera duplicados.
