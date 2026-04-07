using RentMaq.Domain.Enums;

namespace RentMaq.Domain.StateMachines;

/// <summary>
/// State machine del contrato de arrendamiento (ADR-001 Seccion 6).
/// 9 estados, operada exclusivamente por Worker 7.
/// NUNCA consulta equipment.current_status (No-Join Rule).
/// </summary>
public static class ContractStateMachine
{
    private static readonly Dictionary<ContractStatusEnum, HashSet<ContractStatusEnum>> AllowedTransitions = new()
    {
        [ContractStatusEnum.Draft] = [ContractStatusEnum.Reserved],
        [ContractStatusEnum.Reserved] = [ContractStatusEnum.Active],
        [ContractStatusEnum.Active] = [ContractStatusEnum.SuspendedMaintenance, ContractStatusEnum.Returning, ContractStatusEnum.Overdue],
        [ContractStatusEnum.SuspendedMaintenance] = [ContractStatusEnum.Active],
        [ContractStatusEnum.Returning] = [ContractStatusEnum.Inspection],
        [ContractStatusEnum.Inspection] = [ContractStatusEnum.Closed, ContractStatusEnum.InDispute],
        [ContractStatusEnum.Overdue] = [ContractStatusEnum.Closed],
        [ContractStatusEnum.InDispute] = [ContractStatusEnum.Closed],
        [ContractStatusEnum.Closed] = [],
    };

    public static bool CanTransition(ContractStatusEnum from, ContractStatusEnum to)
        => AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
}
