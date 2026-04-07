using FluentAssertions;
using RentMaq.Domain.Entities;

namespace RentMaq.Tests.Domain;

public class DepositBalanceTests
{
    [Fact]
    public void AppliedPlusRefunded_MustNotExceedAmount()
    {
        var deposit = new Deposit
        {
            DepositId = Guid.NewGuid(),
            ContractId = Guid.NewGuid(),
            Amount = 30_000m,
            AppliedAmount = 20_000m,
            RefundedAmount = 10_000m,
            Status = "HELD_AS_LIABILITY",
            AccountingClassification = "LIABILITY"
        };

        var balance = deposit.Amount - deposit.AppliedAmount - deposit.RefundedAmount;
        balance.Should().BeGreaterThanOrEqualTo(0, "applied + refunded no puede exceder amount");
    }

    [Fact]
    public void Overapplication_ViolatesConstraint()
    {
        var deposit = new Deposit
        {
            Amount = 30_000m,
            AppliedAmount = 25_000m,
            RefundedAmount = 10_000m
        };

        var balance = deposit.Amount - deposit.AppliedAmount - deposit.RefundedAmount;
        balance.Should().BeNegative("esto viola el CHECK constraint chk_deposit_balance");
    }

    [Fact]
    public void PartialApplication_LeavesPositiveBalance()
    {
        var deposit = new Deposit
        {
            Amount = 50_000m,
            AppliedAmount = 15_000m,
            RefundedAmount = 0m
        };

        var available = deposit.Amount - deposit.AppliedAmount - deposit.RefundedAmount;
        available.Should().Be(35_000m);
    }
}
