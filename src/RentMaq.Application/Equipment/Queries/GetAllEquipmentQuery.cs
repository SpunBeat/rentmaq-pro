using MediatR;
using RentMaq.Application.Equipment.DTOs;

namespace RentMaq.Application.Equipment.Queries;

public record GetAllEquipmentQuery : IRequest<IReadOnlyList<EquipmentDto>>;
