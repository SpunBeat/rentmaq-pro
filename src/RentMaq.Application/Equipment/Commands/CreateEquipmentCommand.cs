using MediatR;
using RentMaq.Domain.Enums;

namespace RentMaq.Application.Equipment.Commands;

public record CreateEquipmentCommand(
    string AssetTag,
    string SerialNumber,
    string Make,
    string Model,
    int? Year,
    EquipmentTypeEnum EquipmentType,
    decimal? WeightTons,
    decimal? AcquisitionCost,
    string? AempEndpointUrl) : IRequest<Guid>;
