using MediatR;
using RentMaq.Application.Equipment.DTOs;
using RentMaq.Domain.Interfaces;

namespace RentMaq.Application.Equipment.Queries;

public class GetEquipmentByIdHandler : IRequestHandler<GetEquipmentByIdQuery, EquipmentDto?>
{
    private readonly IEquipmentRepository _repository;

    public GetEquipmentByIdHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<EquipmentDto?> Handle(GetEquipmentByIdQuery request, CancellationToken ct)
    {
        var e = await _repository.GetByIdAsync(request.EquipmentId, ct);
        if (e is null) return null;

        return new EquipmentDto(
            e.EquipmentId,
            e.AssetTag,
            e.SerialNumber,
            e.Make,
            e.Model,
            e.Year,
            e.EquipmentType,
            e.CurrentStatus,
            e.WeightTons,
            e.AcquisitionCost);
    }
}
