using RentMaq.Domain.Enums;

namespace RentMaq.Domain.StateMachines;

/// <summary>
/// State machine del activo fisico (ADR-001 Seccion 6).
/// Transiciones operadas exclusivamente por Worker 3.
/// </summary>
public static class EquipmentStateMachine
{
    private static readonly Dictionary<EquipmentStatusEnum, HashSet<EquipmentStatusEnum>> AllowedTransitions = new()
    {
        [EquipmentStatusEnum.Available] = [EquipmentStatusEnum.Rented, EquipmentStatusEnum.InMaintenance, EquipmentStatusEnum.Decommissioned],
        [EquipmentStatusEnum.Rented] = [EquipmentStatusEnum.InMaintenance],
        [EquipmentStatusEnum.InMaintenance] = [EquipmentStatusEnum.Available, EquipmentStatusEnum.Decommissioned],
        [EquipmentStatusEnum.Decommissioned] = [],
    };

    public static bool CanTransition(EquipmentStatusEnum from, EquipmentStatusEnum to)
        => AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);

    /// <summary>
    /// Valida que IN_MAINTENANCE -> AVAILABLE cumpla LOTO gate.
    /// NOM-004-STPS-1999 Art. 7.2.2: sin LOTO aplicado + protectores reinstalados, NO se desbloquea.
    /// </summary>
    public static bool CanExitMaintenance(bool lotoApplied, bool protectorsReinstalled)
        => lotoApplied && protectorsReinstalled;
}
