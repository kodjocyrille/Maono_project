using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Admin.Queries;

/// <summary>
/// List all roles with their permissions.
/// </summary>
public record ListRolesQuery() : IQuery<Result<List<RoleDto>>>;

/// <summary>
/// List all available permissions in the system.
/// </summary>
public record ListPermissionsQuery() : IQuery<Result<List<PermissionDto>>>;
