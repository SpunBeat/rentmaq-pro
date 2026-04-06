using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentMaq.Application.Equipment.Commands;
using RentMaq.Application.Equipment.DTOs;
using RentMaq.Application.Equipment.Queries;

namespace RentMaq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public EquipmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentDto>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllEquipmentQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEquipmentByIdQuery(id), ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateEquipmentCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
}
