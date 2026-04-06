# Runbook: Cargo por Dano a Maquinaria

## Flujo completo: damage_assessment -> deposito -> CFDI

Este procedimiento aplica cuando se confirma que un dano a la maquinaria es atribuible al arrendatario (`attribution = 'TENANT_ATTRIBUTABLE'`). El flujo involucra tres workers en secuencia.

---

## Paso 1: Evaluacion de dano (Manual)

1. El evaluador de patio inspecciona el equipo (post-devolucion o durante visita en obra).
2. Crea un registro en `damage_assessments` con:
   - `attribution = 'TENANT_ATTRIBUTABLE'`
   - `estimated_repair_cost` con el monto estimado
   - Evidencia fotografica en campo `photos` (JSONB)
   - Firma del cliente en `customer_signature_url` (si aplica)
3. Cambia el estado a `status = 'APPROVED_FOR_CHARGE'`.

---

## Paso 2: Impacto financiero (Worker 4 -- FinancialImpactWorker)

El Worker 4 se activa automaticamente al detectar `damage_assessments.status = 'APPROVED_FOR_CHARGE'`.

1. Consulta el deposito disponible del contrato en tabla `deposits`.
2. Calcula la distribucion:
   - Si `estimated_repair_cost <= deposito disponible`: todo se cubre con deposito.
   - Si `estimated_repair_cost > deposito disponible`: `amount_from_deposit = saldo disponible`, `amount_direct_bill = excedente`.
3. Crea registro en `extraordinary_charges` con los montos calculados.
4. Actualiza `deposits.applied_amount` y migra `accounting_classification` de `LIABILITY` a `RECOGNIZED_INCOME`.
5. Publica evento para el Worker 5.

---

## Paso 3: Emision de CFDI (Worker 5 -- CfdiLifecycleWorker)

El Worker 5 emite un **CFDI de Ingreso NUEVO e independiente** (`cfdi_type = 'I'`).

1. Genera el XML con los datos del cargo extraordinario.
2. Timbra ante el PAC (Facturapi, SW Sapiens, etc.).
3. Almacena `uuid_fiscal`, `xml_url`, `pdf_url` en `cfdi_documents`.
4. Actualiza `extraordinary_charges.status = 'INVOICED'` y vincula `cfdi_id`.

---

## Recordatorio critico

**NUNCA cancelar ni sustituir la factura mensual de renta.** La factura que ampara el periodo de arrendamiento y el cargo por dano son eventos fiscales independientes. El CFDI de Ingreso por penalizacion es un comprobante nuevo, no una sustitucion.

- La sustitucion (`relation_type = '04'`) es EXCLUSIVA para correcciones de errores administrativos (RFC incorrecto, monto equivocado).
- La nota de credito (`relation_type = '01'`) es EXCLUSIVA para bonificaciones por falla imputable al arrendador.

Si no sabes a cual de los tres flujos pertenece tu caso, el diseno esta mal. Consulta `CLAUDE.md` Seccion 2.

---

## Que hacer si el dano excede el deposito

El Worker 4 maneja esto automaticamente:

1. Aplica todo el saldo disponible del deposito (`amount_from_deposit = saldo`).
2. El excedente se registra como facturacion directa (`amount_direct_bill = excedente`).
3. El Worker 5 emite el CFDI por el monto total (deposito + excedente).
4. El area de cobranza gestiona el cobro del excedente por la via normal.

---

## Como manejar una disputa del cliente

1. Cambiar `damage_assessments.attribution` a `UNDER_DISPUTE`.
2. El contrato transiciona a `IN_DISPUTE` (Worker 7).
3. El contrato en `IN_DISPUTE` **no bloquea** la operacion del equipo fisico (No-Join Rule). Si el equipo ya fue reparado, puede rentarse a otro cliente aunque la disputa siga abierta.
4. La disputa se resuelve por negociacion o via juridica.
5. Al resolverse: si el cargo procede, retomar desde Paso 1 con `APPROVED_FOR_CHARGE`. Si no procede, cambiar a `DISMISSED`.

---

## Referencia

- `CLAUDE.md` Seccion 2 (Tres Flujos Fiscales Excluyentes).
- `docs/ADR-001-spec.md` Seccion 3.4 (tablas financieras), Seccion 4 (Workers 4 y 5).

---

Ultima actualizacion: 2026-04-06. Responsable: Finanzas / Operaciones. Estado: Vigente.
