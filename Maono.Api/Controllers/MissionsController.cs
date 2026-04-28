using Maono.Api.Common;
using Maono.Application.Features.Missions.Commands;
using Maono.Application.Features.Missions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Contenus & Production")]
public class MissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MissionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create([FromBody] CreateMissionCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Mission créée"));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _mediator.Send(new ListMissionsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetMissionByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMissionCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Error("L'identifiant ne correspond pas.", 400));
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Mission mise à jour"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMissionStatusCommand command)
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
        var result = await _mediator.Send(new DeleteMissionCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Mission archivée"));
    }
}
