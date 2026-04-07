using FluentAssertions;
using RentMaq.Domain.Enums;
using RentMaq.Domain.StateMachines;

namespace RentMaq.Tests.Domain;

public class EquipmentStatusTransitionTests
{
    // --- Transiciones validas ---

    [Fact]
    public void Available_To_Rented_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Available, EquipmentStatusEnum.Rented)
            .Should().BeTrue();

    [Fact]
    public void Available_To_InMaintenance_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Available, EquipmentStatusEnum.InMaintenance)
            .Should().BeTrue();

    [Fact]
    public void Available_To_Decommissioned_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Available, EquipmentStatusEnum.Decommissioned)
            .Should().BeTrue();

    [Fact]
    public void Rented_To_InMaintenance_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Rented, EquipmentStatusEnum.InMaintenance)
            .Should().BeTrue();

    [Fact]
    public void InMaintenance_To_Available_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.InMaintenance, EquipmentStatusEnum.Available)
            .Should().BeTrue();

    [Fact]
    public void InMaintenance_To_Decommissioned_IsAllowed()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.InMaintenance, EquipmentStatusEnum.Decommissioned)
            .Should().BeTrue();

    // --- Transiciones prohibidas ---

    [Fact]
    public void Rented_To_Available_IsForbidden()
        => EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Rented, EquipmentStatusEnum.Available)
            .Should().BeFalse("un equipo rentado no puede volver a disponible sin pasar por mantenimiento");

    [Fact]
    public void Decommissioned_To_Any_IsForbidden()
    {
        foreach (var target in Enum.GetValues<EquipmentStatusEnum>())
        {
            EquipmentStateMachine.CanTransition(EquipmentStatusEnum.Decommissioned, target)
                .Should().BeFalse($"Decommissioned es estado terminal, no puede ir a {target}");
        }
    }

    // --- LOTO Gatekeeper (NOM-004-STPS-1999 Art. 7.2.2) ---

    [Fact]
    public void ExitMaintenance_RequiresBoth_LotoAndProtectors()
        => EquipmentStateMachine.CanExitMaintenance(lotoApplied: true, protectorsReinstalled: true)
            .Should().BeTrue();

    [Fact]
    public void ExitMaintenance_WithoutLoto_IsForbidden()
        => EquipmentStateMachine.CanExitMaintenance(lotoApplied: false, protectorsReinstalled: true)
            .Should().BeFalse("LOTO no aplicado viola NOM-004 Art. 7.2.2");

    [Fact]
    public void ExitMaintenance_WithoutProtectors_IsForbidden()
        => EquipmentStateMachine.CanExitMaintenance(lotoApplied: true, protectorsReinstalled: false)
            .Should().BeFalse("protectores no reinstalados viola NOM-004 Art. 7.2.2");

    [Fact]
    public void ExitMaintenance_WithoutBoth_IsForbidden()
        => EquipmentStateMachine.CanExitMaintenance(lotoApplied: false, protectorsReinstalled: false)
            .Should().BeFalse();
}
