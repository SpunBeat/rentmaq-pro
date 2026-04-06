# Interaccion entre Workers

Diagrama de interaccion entre los 7 workers de RentMaq Pro, agrupados por dominio. Las flechas solidas representan escritura; las punteadas representan lectura. La comunicacion entre dominios se realiza exclusivamente mediante tablas intermedias (No-Join Rule).

---

## Diagrama de Flujo

```mermaid
flowchart TB
    subgraph OPERATIVO["Dominio Operativo (equipment.current_status)"]
        W1[Worker 1: TelemetryIngestionWorker]
        W2[Worker 2: AlertEvaluationWorker]
        W3[Worker 3: MaintenanceOrchestrationWorker]
        W6[Worker 6: ComplianceDocumentWorker]
    end

    subgraph FINANCIERO["Dominio Financiero (rental_contracts.status)"]
        W4[Worker 4: FinancialImpactWorker]
        W5[Worker 5: CfdiLifecycleWorker]
        W7[Worker 7: ContractLifecycleWorker]
    end

    subgraph TABLAS_OPERATIVAS["Tablas Operativas"]
        EQ[(equipment)]
        TR[(telemetry_readings)]
        TA[(telemetry_alerts)]
        MS[(maintenance_schedules)]
        WO[(maintenance_work_orders)]
        ET[(equipment_thresholds)]
        ELP[(equipment_load_profiles)]
        GF[(geofences)]
    end

    subgraph TABLAS_INTERMEDIAS["Tablas Intermedias (puente entre dominios)"]
        DA[(damage_assessments)]
        EC[(extraordinary_charges)]
    end

    subgraph TABLAS_FINANCIERAS["Tablas Financieras"]
        RC[(rental_contracts)]
        CF[(cfdi_documents)]
        DP[(deposits)]
    end

    %% Worker 1: TelemetryIngestionWorker
    W1 -.->|lee| EQ
    W1 -.->|lee| GF
    W1 ==>|escribe| TR
    W1 ==>|escribe| TA

    %% Worker 2: AlertEvaluationWorker
    W2 -.->|lee| TR
    W2 -.->|lee| EQ
    W2 -.->|lee| ET
    W2 -.->|lee| ELP
    W2 ==>|escribe| TA
    W2 -- "evento: CRITICAL/SHUTDOWN" --> W3

    %% Worker 3: MaintenanceOrchestrationWorker
    W3 -.->|lee| MS
    W3 -.->|lee| TA
    W3 ==>|escribe| WO
    W3 ==>|escribe| EQ
    W3 -- "evento: equipo en mantenimiento" --> W7

    %% Worker 6: ComplianceDocumentWorker
    W6 -.->|lee| WO
    W6 ==>|genera PDFs| WO

    %% Worker 4: FinancialImpactWorker
    W4 -.->|lee| DA
    W4 -.->|lee| DP
    W4 ==>|escribe| EC
    W4 ==>|escribe| DP
    W4 -- "evento: cargo listo" --> W5

    %% Worker 5: CfdiLifecycleWorker
    W5 -.->|lee| EC
    W5 -.->|lee| RC
    W5 ==>|escribe| CF

    %% Worker 7: ContractLifecycleWorker
    W7 ==>|escribe| RC
    W7 -.->|lee| DA
    W7 -.->|lee| DP
```

---

## Matriz de Lectura/Escritura por Worker

| Worker | Lee | Escribe |
|--------|-----|---------|
| W1: TelemetryIngestion | `equipment`, `geofences` | `telemetry_readings`, `telemetry_alerts` |
| W2: AlertEvaluation | `telemetry_readings`, `equipment`, `equipment_thresholds`, `equipment_load_profiles` | `telemetry_alerts` |
| W3: MaintenanceOrchestration | `telemetry_alerts`, `maintenance_schedules` | `maintenance_work_orders`, `equipment.current_status` |
| W6: ComplianceDocument | `maintenance_work_orders` | PDFs (bitacora NOM-004, protocolo LOTO) |
| W4: FinancialImpact | `damage_assessments`, `deposits` | `extraordinary_charges`, `deposits` |
| W5: CfdiLifecycle | `extraordinary_charges`, `rental_contracts` | `cfdi_documents` |
| W7: ContractLifecycle | `damage_assessments`, `deposits` | `rental_contracts` |

---

## Reglas de Desacoplamiento

1. **Workers operativos (1, 2, 3, 6):** NUNCA leen `rental_contracts.status`.
2. **Workers financieros (4, 5, 7):** NUNCA leen `equipment.current_status`.
3. La comunicacion entre dominios se realiza mediante tablas intermedias: `telemetry_alerts`, `damage_assessments`, `extraordinary_charges`.
4. Verificacion automatizada:

```bash
# Debe retornar cero resultados
grep -r "current_status" src/RentMaq.Workers/Financial/
grep -r "rental_contracts" src/RentMaq.Workers/Operational/
```

---

Ultima actualizacion: 2026-04-06. Responsable: Arquitectura. Estado: Vigente.
