# CFDI 4.0 -- Los Tres Flujos Fiscales de RentMaq Pro

## Principio rector

Los cargos por danos a la maquinaria NUNCA sustituyen, modifican ni cancelan la factura de renta original. La factura de arrendamiento y la penalizacion por mal uso son eventos fiscalmente independientes. El orquestador financiero opera exclusivamente bajo tres flujos mutuamente excluyentes.

---

## Flujo 1: Cargo por Dano (negligencia del cliente)

### Ejemplo concreto

Una plataforma de tijera sufre dano en el cilindro hidraulico por operacion fuera de especificaciones. El evaluador de patio confirma que el dano es atribuible al arrendatario. El costo de reparacion es de $45,000 MXN. El deposito en garantia es de $30,000 MXN.

### Que sucede en el sistema

| Paso | Accion | Tabla | Worker |
|------|--------|-------|--------|
| 1 | Evaluador crea evaluacion de dano | `damage_assessments` (status = APPROVED_FOR_CHARGE) | Manual |
| 2 | Se calcula el impacto financiero | `extraordinary_charges` (amount = 45000, from_deposit = 30000, direct_bill = 15000) | Worker 4 |
| 3 | Se migra el deposito a Ingreso | `deposits` (accounting_classification = RECOGNIZED_INCOME) | Worker 4 |
| 4 | Se emite CFDI de Ingreso NUEVO | `cfdi_documents` (cfdi_type = 'I', total_amount = 45000) | Worker 5 |

### CFDI generado

- **Tipo:** Ingreso (`I`)
- **Relacion con factura mensual:** Ninguna. Es un comprobante completamente independiente.
- **Metodo de pago:** PPD (Pago en Parcialidades o Diferido) si el excedente se cobra despues.

### Lo que NO se debe hacer

- NO cancelar la factura mensual de renta.
- NO emitir un CFDI de sustitucion (Tipo 04) que incluya el cargo por dano.
- NO mezclar el concepto de renta y el de penalizacion en el mismo comprobante.

---

## Flujo 2: Sustitucion por Error Administrativo

### Ejemplo concreto

Se emitio la factura mensual de un minicargador con el RFC del cliente equivocado (se confundio con otro arrendatario del mismo grupo corporativo).

### Que sucede en el sistema

| Paso | Accion | Tabla | Worker |
|------|--------|-------|--------|
| 1 | Se cancela el CFDI original | `cfdi_documents` (cancel_reason = '01', cancellation_status actualizado) | Worker 5 |
| 2 | Se emite CFDI de sustitucion | `cfdi_documents` (relation_type = '04', related_cfdi_id = CFDI original) | Worker 5 |

### CFDI generado

- **Tipo:** Ingreso (`I`) -- sustitucion del original.
- **Relacion:** `relation_type = '04'` (Sustitucion).
- **Motivo de cancelacion del original:** `cancel_reason = '01'` (Comprobante emitido con errores con relacion).

### Cuando usar este flujo

SOLO para correcciones administrativas:
- RFC incorrecto.
- Monto equivocado por error de captura.
- Codigo postal incorrecto.
- Regimen fiscal incorrecto.

### Cuando NO usar este flujo

- NUNCA para agregar un cargo por dano a una factura existente.
- NUNCA para anular una transaccion completa.

---

## Flujo 3: Bonificacion (falla imputable al arrendador)

### Ejemplo concreto

Un generador asignado a una obra presento falla mecanica por defecto de fabrica. El equipo estuvo fuera de servicio 5 dias. El arrendador debe bonificar al cliente los dias sin servicio ($8,500 MXN de una renta mensual de $51,000 MXN).

### Que sucede en el sistema

| Paso | Accion | Tabla | Worker |
|------|--------|-------|--------|
| 1 | Se emite CFDI de Egreso (Nota de Credito) | `cfdi_documents` (cfdi_type = 'E', relation_type = '01', total_amount = 8500) | Worker 5 |

### CFDI generado

- **Tipo:** Egreso (`E`) -- Nota de Credito.
- **Relacion:** `relation_type = '01'` (Nota de Credito del comprobante relacionado).
- **Monto:** Parcial ($8,500 de los $51,000 mensuales).

### Restriccion critica

NUNCA usar una Nota de Credito para anular la transaccion completa. La Nota de Credito es una bonificacion parcial. Anular el 100% del monto duplicaria el ingreso gravable ante el SAT (el ingreso original ya fue declarado y la nota lo revertiria, pero si luego se re-factura, se reporta dos veces como ingreso).

### Cuando NO usar este flujo

- NUNCA para corregir errores de captura (eso es Flujo 2).
- NUNCA para cobrar un dano al cliente (eso es Flujo 1).

---

## Resumen comparativo

| Escenario | Flujo | Tipo CFDI | Relacion | Impacto en factura mensual | Impacto en deposito |
|-----------|-------|-----------|----------|---------------------------|---------------------|
| Dano por negligencia del cliente | 1 | Ingreso (nuevo) | Ninguna | Sin afectacion | Migra de Pasivo a Ingreso |
| Error en factura original | 2 | Ingreso (sustitucion) | Tipo 04 | Se cancela y reexpide | Sin afectacion |
| Falla del equipo imputable al arrendador | 3 | Egreso (Nota de Credito) | Tipo 01 | Sin cancelacion | Sin afectacion |

---

## Referencia

- `CLAUDE.md` Seccion 2 (Prohibicion Fiscal: Tres Flujos Mutuamente Excluyentes).
- `docs/ADR-001-spec.md` Seccion 3 (Prohibicion estricta de arquitectura) y Seccion 3.4 (tabla `cfdi_documents`).
- `docs/ADR-001-spec.md` Seccion 4, Workers 4 y 5.

---

Ultima actualizacion: 2026-04-06. Responsable: Finanzas / Arquitectura. Estado: Vigente.
