using Maono.Api.Common;
using Maono.Application.Features.Publications.Commands;
using Maono.Application.Features.Publications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Publication")]
public class PublicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PublicationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var result = await _mediator.Send(new ListPublicationsQuery(status));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetPublicationByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost("schedule")]
    [Authorize(Policy = "CanPublish")]
    public async Task<IActionResult> Schedule([FromBody] SchedulePublicationCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Publication planifiée"));
    }

    [HttpPost("{id}/publish")]
    [Authorize(Policy = "CanPublish")]
    public async Task<IActionResult> PublishNow(Guid id)
    {
        var result = await _mediator.Send(new PublishNowCommand(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Publication publiée"));
    }

    [HttpPost("{id}/retry")]
    [Authorize(Policy = "CanPublish")]
    public async Task<IActionResult> Retry(Guid id)
    {
        var result = await _mediator.Send(new RetryPublicationCommand(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Publication relancée"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePublicationCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Publication supprimée"));
    }
}
