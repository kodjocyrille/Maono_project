using Maono.Api.Common;
using Maono.Application.Features.Portal.Commands;
using Maono.Application.Features.Portal.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

/// <summary>
/// Public portal for external clients.
/// All endpoints are [AllowAnonymous] — access is controlled by the ephemeral token.
/// </summary>
[ApiController]
[Route("api/portal")]
[AllowAnonymous]
[Tags("Clients & Portail")]
public class PortalController : ControllerBase
{
    private readonly IMediator _mediator;
    public PortalController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// View all contents accessible with this token.
    /// Used by the client portal frontend to render the page.
    /// </summary>
    [HttpGet("{token}")]
    public async Task<IActionResult> GetContents(string token)
    {
        var result = await _mediator.Send(new GetPortalContentsQuery(token));
        if (!result.IsSuccess) return Unauthorized(ApiResponse<object>.Error(result.Error!, 401));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>
    /// Submit a client decision (approved / changes_requested) for a content item.
    /// </summary>
    [HttpPost("{token}/decisions")]
    public async Task<IActionResult> SubmitDecision(string token, [FromBody] PortalDecisionRequest request)
    {
        var result = await _mediator.Send(new SubmitPortalDecisionCommand(
            token, request.ContentItemId, request.Decision, request.Comment));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Décision soumise"));
    }
}

/// <summary>
/// Internal endpoints for managing portal tokens.
/// Requires authentication.
/// </summary>
[ApiController]
[Route("api/clients/{clientId}/portal-tokens")]
[Authorize]
[Tags("Clients & Portail")]
public class PortalTokensController : ControllerBase
{
    private readonly IMediator _mediator;
    public PortalTokensController(IMediator mediator) => _mediator = mediator;

    /// <summary>Generate a new portal access token for a client.</summary>
    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Generate(Guid clientId, [FromBody] GenerateTokenRequest request)
    {
        var result = await _mediator.Send(new GeneratePortalTokenCommand(
            clientId, request.ContentItemId, request.CampaignId, request.ExpiryHours));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Token portail généré"));
    }

    /// <summary>Revoke an existing portal access token.</summary>
    [HttpDelete("{tokenId}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Revoke(Guid clientId, Guid tokenId, [FromBody] RevokeTokenRequest? request)
    {
        var result = await _mediator.Send(new RevokePortalTokenCommand(tokenId, request?.Reason));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Token révoqué"));
    }
}

public record PortalDecisionRequest(Guid ContentItemId, string Decision, string? Comment);
public record GenerateTokenRequest(Guid? ContentItemId, Guid? CampaignId, int ExpiryHours = 72);
public record RevokeTokenRequest(string? Reason);
