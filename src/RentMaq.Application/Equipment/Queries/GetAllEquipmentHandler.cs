using MediatR;
using RentMaq.Application.Equipment.DTOs;
using RentMaq.Domain.Interfaces;

namespace RentMaq.Application.Equipment.Queries;

public class GetAllEquipmentHandler : IRequestHandler<GetAllEquipmentQuery, IReadOnlyList<EquipmentDto>>
{
    private readonly IEquipmentRepository _repository;

    public GetAllEquipmentHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<EquipmentDto>> Handle(GetAllEquipmentQuery request, CancellationToken ct)
    {
        var equipment = await _repository.GetAllAsync(ct);

        return equipment.Select(e => new EquipmentDto(
            e.EquipmentId,
            e.AssetTag,
            e.SerialNumber,
            e.Make,
            e.Model,
            e.Year,
            e.EquipmentType,
            e.CurrentStatus,
            e.WeightTons,
            e.AcquisitionCost)).ToList();
    }
}
