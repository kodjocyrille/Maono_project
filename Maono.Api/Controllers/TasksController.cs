using Maono.Api.Common;
using Maono.Application.Features.Tasks.Commands;
using Maono.Application.Features.Tasks.Queries;
using Maono.Domain.Content.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Authorize]
[Tags("Contenus & Production")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    public TasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Retourne toutes les tâches assignées à l'utilisateur connecté.
    /// Filtre optionnel : ?status=0 (Pending) | 1 (InProgress) | 2 (Completed) | 3 (Blocked)
    /// </summary>
    [HttpGet("api/tasks/my")]
    public async Task<IActionResult> GetMyTasks([FromQuery] ContentTaskStatus? status)
    {
        var result = await _mediator.Send(new GetMyTasksQuery(status));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }


    [HttpGet("api/contents/{contentId}/tasks")]
    public async Task<IActionResult> List(Guid contentId)
    {
        var result = await _mediator.Send(new ListTasksQuery(contentId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }


    [HttpPost("api/contents/{contentId}/tasks")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create(Guid contentId, [FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand(
            contentId, request.Title, request.Description,
            request.AssignedToUserId, request.Priority, request.DueDate);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Tâche créée"));
    }

    [HttpPatch("api/tasks/{taskId}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid taskId, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand(
            taskId, request.Title, request.Description, request.Status,
            request.Priority, request.AssignedToUserId, request.DueDate, request.BlockedReason);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("api/tasks/{taskId}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid taskId)
    {
        var result = await _mediator.Send(new DeleteTaskCommand(taskId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Tâche supprimée"));
    }
}

public record CreateTaskRequest(
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    ContentTaskPriority Priority,
    DateTime? DueDate);

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    ContentTaskStatus? Status,
    ContentTaskPriority? Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    string? BlockedReason);
