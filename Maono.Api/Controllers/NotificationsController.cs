using Maono.Api.Common;
using Maono.Application.Features.Notifications.Commands;
using Maono.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _mediator.Send(new ListNotificationsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("All notifications marked as read"));
    }

    // ── ECR-024/025 — Notification Preferences ─────────────

    /// <summary>Get notification preferences for current user.</summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var result = await _mediator.Send(new Application.Features.Notifications.GetNotificationPreferencesQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Update a notification preference channel.</summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreference([FromBody] UpdatePreferenceRequest request)
    {
        var result = await _mediator.Send(new Application.Features.Notifications.UpdateNotificationPreferenceCommand(
            request.Channel, request.Enabled, request.DigestMode));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}

public record UpdatePreferenceRequest(
    Maono.Domain.Notifications.Enums.NotificationChannel Channel,
    bool Enabled,
    string? DigestMode
);
