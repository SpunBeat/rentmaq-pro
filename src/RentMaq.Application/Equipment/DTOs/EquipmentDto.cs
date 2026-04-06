using RentMaq.Domain.Enums;

namespace RentMaq.Application.Equipment.DTOs;

public record EquipmentDto(
    Guid EquipmentId,
    string AssetTag,
    string SerialNumber,
    string Make,
    string Model,
    int? Year,
    EquipmentTypeEnum EquipmentType,
    EquipmentStatusEnum CurrentStatus,
    decimal? WeightTons,
    decimal? AcquisitionCost);
