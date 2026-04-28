using Maono.Api.Common;
using Maono.Application.Common.Interfaces;
using Maono.Application.Features.Workspaces.Commands;
using Maono.Application.Features.Workspaces.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Workspace & Membres")]
public class WorkspacesController : ControllerBase
{
    private readonly IMediator _mediator;
    public WorkspacesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Workspace créé"));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _mediator.Send(new ListWorkspacesQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetWorkspaceByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPatch("{id}/settings")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateSettingsRequest request)
    {
        var result = await _mediator.Send(new UpdateWorkspaceSettingsCommand(id, request.SettingsJson, request.DefaultTimezone, request.LogoUrl));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
    /// <summary>
    /// Invite un utilisateur à rejoindre ce workspace.
    /// Tout membre authentifié peut inviter.
    /// </summary>
    [HttpPost("{id}/invite")]
    public async Task<IActionResult> InviteUser(
        Guid id,
        [FromBody] WorkspaceInviteRequest request,
        [FromServices] IInviteTokenService inviteService)
    {
        // Get workspace name for the token
        var wsResult = await _mediator.Send(new GetWorkspaceByIdQuery(id));
        if (!wsResult.IsSuccess)
            return NotFound(ApiResponse<object>.Error("Workspace introuvable.", 404));

        var workspaceName = wsResult.Value!.Name;
        var token = await inviteService.GenerateTokenAsync(
            id, workspaceName, request.Email, request.RoleName);

        return Ok(ApiResponse<object>.Ok(new
        {
            inviteToken = token,
            email = request.Email,
            workspaceId = id,
            workspaceName,
            roleName = request.RoleName,
            expiresInDays = 7
        }, "Invitation générée avec succès."));
    }
}

public record UpdateSettingsRequest(string? SettingsJson, string? DefaultTimezone, string? LogoUrl);
public record WorkspaceInviteRequest(string Email, string RoleName);
