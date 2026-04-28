namespace Maono.Application.Features.Admin;

public record UserAdminDto(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsActive,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc,
    List<UserMembershipDto> Memberships
);

public record UserMembershipDto(
    Guid WorkspaceId,
    string WorkspaceName,
    string RoleName,
    bool IsDefault,
    List<string> Permissions
);
