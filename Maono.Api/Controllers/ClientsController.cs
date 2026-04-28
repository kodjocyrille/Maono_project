using Maono.Api.Common;
using Maono.Application.Features.Clients.Commands;
using Maono.Application.Features.Clients.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Clients & Portail")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ClientsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Client créé"));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search)
    {
        var result = await _mediator.Send(new ListClientsQuery(search));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetClientByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Error("L'identifiant ne correspond pas.", 400));
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteClientCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Client archivé"));
    }
}
