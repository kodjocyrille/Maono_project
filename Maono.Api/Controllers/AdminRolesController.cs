using Maono.Api.Common;
using Maono.Application.Features.Admin.Commands;
using Maono.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "AdminOnly")]
[Tags("Administration")]
public class AdminRolesController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminRolesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lister tous les rôles avec leurs permissions.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListRoles()
    {
        var result = await _mediator.Send(new ListRolesQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>
    /// Lister toutes les permissions disponibles dans le système.
    /// </summary>
    [HttpGet("/api/admin/permissions")]
    public async Task<IActionResult> ListPermissions()
    {
        var result = await _mediator.Send(new ListPermissionsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>
    /// Créer un rôle custom avec des permissions spécifiques.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Rôle créé avec succès."));
    }

    /// <summary>
    /// Modifier les permissions d'un rôle existant.
    /// </summary>
    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> UpdatePermissions(Guid roleId, [FromBody] UpdatePermissionsRequest request)
    {
        var result = await _mediator.Send(new UpdateRolePermissionsCommand(roleId, request.PermissionCodes));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Permissions mises à jour."));
    }

    /// <summary>
    /// Supprimer un rôle custom (les rôles système ne peuvent pas être supprimés).
    /// </summary>
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var result = await _mediator.Send(new DeleteRoleCommand(roleId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Rôle supprimé avec succès."));
    }

    /// <summary>
    /// Assigner un rôle à un utilisateur dans un workspace donné.
    /// Si l'utilisateur a déjà un rôle dans ce workspace, il sera remplacé.
    /// Si c'est sa première membership, elle sera marquée comme défaut.
    /// </summary>
    [HttpPut("/api/admin/users/{userId}/workspaces/{workspaceId}/role")]
    public async Task<IActionResult> AssignRole(Guid userId, Guid workspaceId, [FromBody] AssignRoleRequest request)
    {
        var result = await _mediator.Send(new AssignUserRoleCommand(userId, workspaceId, request.RoleName));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Rôle assigné avec succès."));
    }
}

public record UpdatePermissionsRequest(List<string> PermissionCodes);
public record AssignRoleRequest(string RoleName);
