using Maono.Api.Common;
using Maono.Application.Features.Publications;
using Maono.Domain.Publications.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

/// <summary>
/// ECR-010 — Gestion des connexions sociales (Instagram, Facebook, LinkedIn).
/// </summary>
[ApiController]
[Route("api/social-connections")]
[Authorize]
[Tags("Publication")]
public class SocialConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SocialConnectionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Connect a social account.</summary>
    [HttpPost]
    [Authorize(Policy = "CanPublish")]
    public async Task<IActionResult> Connect([FromBody] ConnectSocialRequest request)
    {
        var result = await _mediator.Send(new ConnectSocialAccountCommand(
            request.Platform, request.ExternalAccountId, request.AccountName,
            request.AccessTokenRef, request.RefreshTokenRef));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Compte connecté"));
    }

    /// <summary>List all social connections.</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _mediator.Send(new ListSocialConnectionsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Disconnect a social account.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanPublish")]
    public async Task<IActionResult> Disconnect(Guid id)
    {
        var result = await _mediator.Send(new DisconnectSocialAccountCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Compte déconnecté"));
    }
}

public record ConnectSocialRequest(
    SocialPlatform Platform,
    string ExternalAccountId,
    string? AccountName,
    string? AccessTokenRef,
    string? RefreshTokenRef
);
