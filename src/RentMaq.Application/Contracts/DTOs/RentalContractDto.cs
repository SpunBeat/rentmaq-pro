using RentMaq.Domain.Enums;

namespace RentMaq.Application.Contracts.DTOs;

public record RentalContractDto(
    Guid ContractId,
    Guid TenantId,
    Guid EquipmentId,
    ContractStatusEnum Status,
    DateOnly StartDate,
    DateOnly? EndDate,
    BillingModelEnum BillingModel,
    decimal BaseMonthlyRate);
