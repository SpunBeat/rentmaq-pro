# Runbook: Alerta GEOFENCE_BREACH

## Que significa esta alerta

El Worker 1 (TelemetryIngestionWorker) detecto que la posicion GPS de un equipo se encuentra fuera del poligono autorizado (`geofences.perimeter`). Se inserto un registro en `telemetry_alerts` con `alert_type = 'GEOFENCE_BREACH'`.

**La alerta NO transiciona el contrato.** El estado del contrato (`rental_contracts.status`) permanece sin cambios. Esto es por diseno -- ver `CLAUDE.md` Seccion 4.

---

## Paso 1: Evaluar si es un falso positivo

El GPS industrial tiene precision CEP95 de aproximadamente 6.2 metros. En zonas de construccion, los valores HDOP/PDOP pueden degradarse significativamente.

1. Consultar la lectura en `telemetry_readings` que genero la alerta.
2. Revisar el campo `hdop`. Valores superiores a 5.0 indican baja precision -- probable falso positivo.
3. Revisar el campo `satellites`. Menos de 4 satelites indica un fix degradado.
4. Consultar las ultimas 5 lecturas del equipo. Si las anteriores estan dentro del poligono y la siguiente tambien regresa, fue un spike transitorio.

**Si es falso positivo:** Marcar la alerta como `acknowledged = TRUE` con nota explicativa. No se requiere accion adicional.

---

## Paso 2: Confirmar salida real

Si la lectura tiene buen HDOP (< 3.0), buen numero de satelites (>= 6) y las lecturas subsecuentes confirman que el equipo esta fuera del poligono:

1. **Contactar al arrendatario** para confirmar si el movimiento fue autorizado.
2. Si fue autorizado (traslado entre obras, por ejemplo): documentar en la alerta y actualizar el geofence si aplica.
3. Si NO fue autorizado: continuar con el Paso 3.

---

## Paso 3: Movimiento no autorizado

1. **Notificar al gerente de operaciones** inmediatamente.
2. **Evaluar el riesgo:** Determinar si es un error del operador (equipo en obra cercana) o un posible robo.
3. **Si es error del operador:** Contactar al arrendatario para que regrese el equipo al poligono autorizado. Documentar el incidente.
4. **Si hay sospecha de robo:**
   - Activar protocolo de recuperacion de activos.
   - Notificar a la aseguradora (campo `insurance_policy_number` del contrato).
   - Presentar denuncia con las coordenadas GPS como evidencia.

---

## Recordatorio critico

La alerta `GEOFENCE_BREACH` es informativa. El sistema **NUNCA** transiciona automaticamente el contrato a `RETURNING` por una lectura GPS. La transicion a `RETURNING` requiere siempre accion humana intencionada (cliente en Portal o chofer en App). La unica excepcion es la recoleccion forzada por mora a dia 60.

---

## Referencia

- `CLAUDE.md` Seccion 4 (Geofencing = Alerta, NUNCA Trigger).
- `docs/ADR-001-spec.md` Seccion 4, Worker 1.

---

Ultima actualizacion: 2026-04-06. Responsable: Operaciones. Estado: Vigente.
