using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentMaq.Application.Contracts.DTOs;
using RentMaq.Application.Contracts.Queries;

namespace RentMaq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("by-tenant/{tenantId:guid}")]
    public async Task<ActionResult<IReadOnlyList<RentalContractDto>>> GetByTenant(Guid tenantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractsByTenantQuery(tenantId), ct);
        return Ok(result);
    }
}
