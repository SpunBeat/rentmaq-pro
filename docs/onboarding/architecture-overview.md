# Panorama de Arquitectura para Nuevos Desarrolladores

Este documento explica las decisiones de diseno fundamentales de RentMaq Pro. No repite el detalle tecnico del ADR-001 -- explica el **por que** detras de cada decision para que puedas tomar decisiones informadas al escribir codigo.

---

## Por que dos state machines independientes

El sistema tiene dos maquinas de estado concurrentes:

- **`equipment.current_status`** -- Estado fisico del activo: AVAILABLE, RENTED, IN_MAINTENANCE, DECOMMISSIONED.
- **`rental_contracts.status`** -- Estado financiero del contrato: DRAFT, RESERVED, ACTIVE, ..., CLOSED.

**La razon:** Un equipo reparado debe poder rentarse a un nuevo cliente aunque el contrato anterior siga en `IN_DISPUTE`. Si las dos state machines estuvieran acopladas, una disputa de cobranza (que puede durar meses) bloquearia la monetizacion del activo fisico. El equipo estaria parado en el patio generando costo y depreciacion mientras el area juridica resuelve una factura.

**Consecuencia en el codigo:** Los workers operativos (1, 2, 3, 6) nunca leen `rental_contracts.status`. Los workers financieros (4, 5, 7) nunca leen `equipment.current_status`. La comunicacion entre dominios se realiza mediante tablas intermedias (`telemetry_alerts`, `damage_assessments`, `extraordinary_charges`).

---

## Por que tres flujos fiscales y no uno solo

La normativa fiscal mexicana (CFDI 4.0) requiere que cada comprobante tenga un proposito claro y trazable. Mezclar conceptos en un solo CFDI genera problemas ante el SAT.

| Escenario | Flujo | Por que no se puede mezclar |
|-----------|-------|---------------------------|
| Cliente dano una plataforma | CFDI de Ingreso nuevo e independiente | La factura mensual ya se emitio correctamente. Cancelarla y reemitirla con el cargo adicional duplicaria el ingreso gravable de la renta |
| RFC equivocado en factura | Sustitucion (cancel + reemision Tipo 04) | Solo aplica para correcciones administrativas, nunca para agregar conceptos nuevos |
| Generador fallo por defecto del arrendador | Nota de Credito (TipoRelacion 01) | Bonificacion parcial, nunca anulacion completa de la transaccion |

**Consecuencia en el codigo:** Si estas escribiendo codigo que toca `cfdi_documents`, preguntate a cual de los tres flujos pertenece. Si no encaja limpiamente en uno solo, el diseno esta mal.

---

## Por que el mantenimiento tiene LOTO y protectores como hard gates

La NOM-004-STPS-1999 exige que al realizar mantenimiento en maquinaria:

1. Se aplique el protocolo LOTO (Lockout/Tagout) para bloquear todas las fuentes de energia.
2. Se reinstalen los protectores de seguridad antes de entregar el equipo.

Omitir cualquiera de estos pasos pone en riesgo la vida del siguiente operador. No es un tema de UX ni de agilidad operativa -- es un tema de responsabilidad penal.

**Consecuencia en el codigo:** La transicion `IN_MAINTENANCE -> AVAILABLE` requiere `loto_applied = TRUE` AND `protectors_reinstalled = TRUE` verificados en base de datos. No existe bypass administrativo. No existe desbloqueo por timeout. Si la orden excede `loto_timeout_hours`, se alerta al supervisor pero el equipo permanece bloqueado hasta verificacion fisica.

---

## Por que el geofencing es alerta y no trigger

Cuando el GPS reporta que un equipo salio del poligono autorizado, el sistema inserta una alerta `GEOFENCE_BREACH` en `telemetry_alerts`. No transiciona el contrato a `RETURNING`.

**La razon:** El GPS industrial tiene precision CEP95 de aproximadamente 6.2 metros. En zonas de construccion (estructuras metalicas, muros de concreto, maquinaria pesada), los valores HDOP/PDOP se degradan significativamente, generando lecturas erraticas. Si cada spike GPS disparara una transicion contractual, tendriamos contratos moviendose a `RETURNING` varias veces por semana por falsos positivos.

**Consecuencia en el codigo:** La transicion a `RETURNING` requiere siempre accion humana intencionada (cliente en Portal o chofer en App). La unica excepcion es la recoleccion forzada por mora a dia 60, que es un proceso administrativo, no telematico.

---

## Flujo de extremo a extremo: Sensor -> CFDI

Este es el flujo completo cuando un sensor detecta un evento de impacto severo atribuible al cliente:

```
Sensor de impacto          Worker 1 (Telemetria)
registra +/-g         -->  Ingesta lectura en telemetry_readings
en equipo rentado          Evalua geofence (ST_Covers)
                                    |
                                    v
                           Worker 2 (Alertas)
                           Compara vs equipment_thresholds
                           Severidad = CRITICAL
                           Inserta en telemetry_alerts
                                    |
                                    v
                           Worker 3 (Mantenimiento)
                           Crea maintenance_work_order
                           Transiciona equipment -> IN_MAINTENANCE
                           Publica evento
                                    |
                                    v
                           Worker 7 (Contrato) [via evento]
                           Transiciona contrato -> SUSPENDED_MAINTENANCE
                                    |
                           Evaluador de patio (manual)
                           Crea damage_assessment
                           attribution = TENANT_ATTRIBUTABLE
                           status = APPROVED_FOR_CHARGE
                                    |
                                    v
                           Worker 4 (Financiero)
                           Calcula cargo vs deposito
                           Migra deposito de Pasivo a Ingreso
                           Crea extraordinary_charge
                                    |
                                    v
                           Worker 5 (CFDI)
                           Emite CFDI de Ingreso NUEVO
                           Timbra ante PAC
                           Genera XML + PDF
```

Cada paso es trazable. Un impacto registrado por sensor puede seguirse hasta el PDF del protocolo LOTO y el XML del CFDI timbrado. La orquestacion no es sincronica -- el timbrado ante el PAC puede fallar temporalmente -- pero garantiza completitud eventual con trazabilidad completa.

---

Ultima actualizacion: 2026-04-06. Responsable: Arquitectura. Estado: Vigente.
