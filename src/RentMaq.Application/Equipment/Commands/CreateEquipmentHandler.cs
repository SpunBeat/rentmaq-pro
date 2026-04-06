using MediatR;
using RentMaq.Domain.Interfaces;

namespace RentMaq.Application.Equipment.Commands;

public class CreateEquipmentHandler : IRequestHandler<CreateEquipmentCommand, Guid>
{
    private readonly IEquipmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEquipmentHandler(IEquipmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEquipmentCommand request, CancellationToken ct)
    {
        var entity = new Domain.Entities.Equipment
        {
            EquipmentId = Guid.NewGuid(),
            AssetTag = request.AssetTag,
            SerialNumber = request.SerialNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            EquipmentType = request.EquipmentType,
            WeightTons = request.WeightTons,
            AcquisitionCost = request.AcquisitionCost,
            AempEndpointUrl = request.AempEndpointUrl,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return entity.EquipmentId;
    }
}
