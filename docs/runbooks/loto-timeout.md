# Runbook: Timeout LOTO

## Que significa esta alerta

El sistema detecta que una orden de mantenimiento lleva en estado `LOTO_APPLIED` mas tiempo del configurado en `loto_timeout_hours` (por defecto 24 horas). Esto indica que el protocolo de bloqueo de energia (Lockout/Tagout) fue aplicado pero la orden no se ha completado dentro del tiempo esperado.

El sistema genera una alerta al supervisor de patio. **El equipo NO se desbloquea automaticamente.** Esto es un requisito de NOM-004-STPS-1999 Art. 7.2.2 -- el desbloqueo sin verificacion fisica pone en riesgo la vida del siguiente operador.

---

## Quien debe actuar

- **Responsable primario:** Supervisor de patio.
- **Escalacion:** Gerente de operaciones si el supervisor no responde en 2 horas.

---

## Pasos para resolver

1. **Verificar fisicamente el equipo.** Ir al sitio donde se encuentra la maquinaria y confirmar el estado real con el tecnico asignado.

2. **Si el trabajo esta en progreso:** El tecnico esta trabajando y necesita mas tiempo. Documentar la razon del retraso en `technician_notes` de la orden de trabajo. No se requiere accion en el sistema -- el equipo permanece bloqueado hasta que el tecnico complete el protocolo.

3. **Si el trabajo esta completo pero no se registro en el sistema:**
   - Verificar fisicamente que el LOTO fue retirado correctamente.
   - Verificar fisicamente que los protectores de seguridad fueron reinstalados.
   - Registrar en el sistema: `loto_applied = TRUE`, `protectors_reinstalled = TRUE`.
   - Cerrar la orden de trabajo.

4. **Si el tecnico abandono el trabajo:** Asignar un nuevo tecnico. La orden permanece activa y el equipo permanece en `IN_MAINTENANCE` hasta completar el protocolo completo.

---

## Que NO hacer

- **NUNCA desbloquear el equipo sin verificacion fisica.** No existe bypass administrativo ni endpoint que salte esta validacion.
- **NUNCA asumir que un timeout significa que el trabajo esta completo.** El timeout es una alerta para investigar, no un trigger de transicion.
- **NUNCA crear workarounds en codigo** que permitan transicionar de `IN_MAINTENANCE` a `AVAILABLE` sin cumplir ambas condiciones (`loto_applied` y `protectors_reinstalled`).

---

## Referencia normativa

- NOM-004-STPS-1999, Articulo 7.2.2 incisos a) y c).
- `CLAUDE.md` Seccion 3 (LOTO Gatekeeper).

---

Ultima actualizacion: 2026-04-06. Responsable: Operaciones. Estado: Vigente.
