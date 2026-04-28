using Maono.Api.Common;
using Maono.Application.Features.Planning.Commands;
using Maono.Application.Features.Planning.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Publication")]
public class CalendarController : ControllerBase
{
    private readonly IMediator _mediator;
    public CalendarController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? campaignId)
    {
        var result = await _mediator.Send(new ListCalendarEntriesQuery(campaignId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("capacity")]
    public async Task<IActionResult> GetCapacity([FromQuery] DateTime weekStart)
    {
        var result = await _mediator.Send(new GetResourceCapacityQuery(weekStart));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create([FromBody] CreateCalendarEntryCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Entrée calendrier créée"));
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCalendarEntryCommand command)
    {
        var result = await _mediator.Send(command with { Id = id });
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCalendarEntryCommand(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Entrée supprimée"));
    }

    [HttpPost("{id}/validate")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Validate(Guid id)
    {
        var result = await _mediator.Send(new ValidateCalendarEntryCommand(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Entrée validée"));
    }

    /// <summary>ECR-021 — Calendar risk indicators (overdue, blocked tasks).</summary>
    [HttpGet("risks")]
    public async Task<IActionResult> GetRisks()
    {
        var result = await _mediator.Send(new GetCalendarRisksQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    // ── ECR-034 — Saved Views ───────────────────────────────

    /// <summary>Create a saved view with persisted filters.</summary>
    [HttpPost("views")]
    public async Task<IActionResult> CreateView([FromBody] CreateSavedViewRequest request)
    {
        var result = await _mediator.Send(new Application.Features.Planning.CreateSavedViewCommand(request.Name, request.FiltersJson));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Vue sauvegardée"));
    }

    /// <summary>List saved views for current user.</summary>
    [HttpGet("views")]
    public async Task<IActionResult> ListViews()
    {
        var result = await _mediator.Send(new Application.Features.Planning.ListSavedViewsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Delete a saved view.</summary>
    [HttpDelete("views/{viewId}")]
    public async Task<IActionResult> DeleteView(Guid viewId)
    {
        var result = await _mediator.Send(new Application.Features.Planning.DeleteSavedViewCommand(viewId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Vue supprimée"));
    }
}

public record CreateSavedViewRequest(string Name, string? FiltersJson);
