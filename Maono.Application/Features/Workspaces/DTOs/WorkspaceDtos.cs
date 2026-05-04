namespace Maono.Application.Features.Workspaces.DTOs;

public record WorkspaceDto(
    Guid Id,
    string Name,
    string Slug,
    string? Plan,
    string? DefaultTimezone,
    string? LogoUrl,
    DateTime CreatedAtUtc
);

public record WorkspaceDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? Plan,
    string? DefaultTimezone,
    string? LogoUrl,
    string? SettingsJson,
    DateTime CreatedAtUtc,
    List<WorkspaceMemberDto> Members
);

public record WorkspaceMemberDto(Guid MembershipId, Guid UserId, string DisplayName, string RoleName, DateTime JoinedAtUtc);

public record MemberDto(
    Guid MembershipId,
    Guid UserId,
    string DisplayName,
    string Email,
    string RoleName,
    string Status,
    DateTime JoinedAtUtc
);

public record InviteMemberResultDto(
    MemberDto Member,
    string InviteToken
);

public record MyWorkspaceDto(
    Guid WorkspaceId,
    string Name,
    string Slug,
    string? Plan,
    string? LogoUrl,
    string RoleName,
    bool IsDefault,
    DateTime JoinedAtUtc
);
