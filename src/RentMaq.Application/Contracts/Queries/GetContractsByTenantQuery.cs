using MediatR;
using RentMaq.Application.Contracts.DTOs;

namespace RentMaq.Application.Contracts.Queries;

public record GetContractsByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<RentalContractDto>>;
