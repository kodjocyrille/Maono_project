using Maono.Api.Common;
using Maono.Application.Features.Workspaces.Commands;
using Maono.Application.Features.Workspaces.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId}/members")]
[Authorize]
[Tags("Workspace & Membres")]
public class MembershipsController : ControllerBase
{
    private readonly IMediator _mediator;
    public MembershipsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(Guid workspaceId)
    {
        var result = await _mediator.Send(new ListMembersQuery(workspaceId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>
    /// Inviter un membre — crée une membership avec statut "Invited".
    /// L'utilisateur ciblé devra accepter l'invitation via POST .../accept.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Invite(Guid workspaceId, [FromBody] InviteMemberRequest request)
    {
        var result = await _mediator.Send(new InviteMemberCommand(workspaceId, request.Email, request.RoleName));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Invitation envoyée. L'utilisateur doit accepter pour rejoindre le workspace."));
    }

    /// <summary>
    /// Accepter une invitation — passe la membership de "Invited" à "Active".
    /// Seul l'utilisateur ciblé peut accepter son invitation.
    /// </summary>
    [HttpPost("{membershipId}/accept")]
    public async Task<IActionResult> Accept(Guid workspaceId, Guid membershipId)
    {
        var result = await _mediator.Send(new AcceptInvitationCommand(membershipId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Invitation acceptée. Vous êtes maintenant membre du workspace."));
    }

    [HttpPatch("{membershipId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateRole(Guid workspaceId, Guid membershipId, [FromBody] UpdateRoleRequest request)
    {
        var result = await _mediator.Send(new UpdateMemberRoleCommand(membershipId, request.RoleName));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("{membershipId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Remove(Guid workspaceId, Guid membershipId)
    {
        var result = await _mediator.Send(new RemoveMemberCommand(membershipId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Membre retiré du workspace"));
    }
}

public record InviteMemberRequest(string Email, string RoleName);
public record UpdateRoleRequest(string RoleName);
