using FluentAssertions;
using RentMaq.Domain.Enums;
using RentMaq.Domain.StateMachines;

namespace RentMaq.Tests.Domain;

public class ContractStatusTransitionTests
{
    // --- Transiciones validas (ADR-001 Seccion 6) ---

    [Theory]
    [InlineData(ContractStatusEnum.Draft, ContractStatusEnum.Reserved)]
    [InlineData(ContractStatusEnum.Reserved, ContractStatusEnum.Active)]
    [InlineData(ContractStatusEnum.Active, ContractStatusEnum.SuspendedMaintenance)]
    [InlineData(ContractStatusEnum.Active, ContractStatusEnum.Returning)]
    [InlineData(ContractStatusEnum.Active, ContractStatusEnum.Overdue)]
    [InlineData(ContractStatusEnum.SuspendedMaintenance, ContractStatusEnum.Active)]
    [InlineData(ContractStatusEnum.Returning, ContractStatusEnum.Inspection)]
    [InlineData(ContractStatusEnum.Inspection, ContractStatusEnum.Closed)]
    [InlineData(ContractStatusEnum.Inspection, ContractStatusEnum.InDispute)]
    [InlineData(ContractStatusEnum.InDispute, ContractStatusEnum.Closed)]
    [InlineData(ContractStatusEnum.Overdue, ContractStatusEnum.Closed)]
    public void ValidTransitions_AreAllowed(ContractStatusEnum from, ContractStatusEnum to)
        => ContractStateMachine.CanTransition(from, to).Should().BeTrue();

    // --- Transiciones prohibidas ---

    [Theory]
    [InlineData(ContractStatusEnum.Draft, ContractStatusEnum.Active)]
    [InlineData(ContractStatusEnum.Draft, ContractStatusEnum.Closed)]
    [InlineData(ContractStatusEnum.Reserved, ContractStatusEnum.Returning)]
    [InlineData(ContractStatusEnum.Active, ContractStatusEnum.Draft)]
    [InlineData(ContractStatusEnum.Returning, ContractStatusEnum.Active)]
    [InlineData(ContractStatusEnum.Inspection, ContractStatusEnum.Returning)]
    public void InvalidTransitions_AreForbidden(ContractStatusEnum from, ContractStatusEnum to)
        => ContractStateMachine.CanTransition(from, to).Should().BeFalse();

    [Fact]
    public void Closed_IsTerminal()
    {
        foreach (var target in Enum.GetValues<ContractStatusEnum>())
        {
            ContractStateMachine.CanTransition(ContractStatusEnum.Closed, target)
                .Should().BeFalse($"Closed es estado terminal, no puede ir a {target}");
        }
    }
}
