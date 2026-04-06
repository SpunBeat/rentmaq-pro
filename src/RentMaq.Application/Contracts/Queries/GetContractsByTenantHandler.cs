using MediatR;
using RentMaq.Application.Contracts.DTOs;
using RentMaq.Domain.Interfaces;

namespace RentMaq.Application.Contracts.Queries;

public class GetContractsByTenantHandler : IRequestHandler<GetContractsByTenantQuery, IReadOnlyList<RentalContractDto>>
{
    private readonly IRentalContractRepository _repository;

    public GetContractsByTenantHandler(IRentalContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RentalContractDto>> Handle(GetContractsByTenantQuery request, CancellationToken ct)
    {
        var contracts = await _repository.GetByTenantAsync(request.TenantId, ct);

        return contracts.Select(c => new RentalContractDto(
            c.ContractId,
            c.TenantId,
            c.EquipmentId,
            c.Status,
            c.StartDate,
            c.EndDate,
            c.BillingModel,
            c.BaseMonthlyRate)).ToList();
    }
}
