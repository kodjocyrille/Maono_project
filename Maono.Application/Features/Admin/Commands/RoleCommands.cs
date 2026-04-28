using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Admin.Commands;

/// <summary>
/// Create a custom role with specific permissions.
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description,
    List<string> PermissionCodes
) : ICommand<Result<RoleDto>>;

/// <summary>
/// Update a role's permissions.
/// </summary>
public record UpdateRolePermissionsCommand(
    Guid RoleId,
    List<string> PermissionCodes
) : ICommand<Result<RoleDto>>;

/// <summary>
/// Delete a custom role (system roles cannot be deleted).
/// </summary>
public record DeleteRoleCommand(Guid RoleId) : ICommand<Result>;

/// <summary>
/// Assign a role to a user in a specific workspace.
/// </summary>
public record AssignUserRoleCommand(
    Guid UserId,
    Guid WorkspaceId,
    string RoleName
) : ICommand<Result<UserMembershipDto>>;
