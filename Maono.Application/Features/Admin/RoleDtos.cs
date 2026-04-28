namespace Maono.Application.Features.Admin;

/// <summary>
/// DTO for role listing with permissions.
/// </summary>
public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    List<string> Permissions
);

/// <summary>
/// DTO for available permission listing.
/// </summary>
public record PermissionDto(
    Guid Id,
    string Code,
    string? Description
);
