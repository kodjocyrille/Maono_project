using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.DTOs;

namespace Maono.Application.Features.Workspaces.Commands;

public record InviteMemberCommand(
    Guid WorkspaceId,
    string Email,
    string RoleName
) : ICommand<Result<InviteMemberResultDto>>;

public record AcceptInvitationCommand(
    Guid MembershipId
) : ICommand<Result<MemberDto>>;

public record UpdateMemberRoleCommand(
    Guid MembershipId,
    string NewRoleName
) : ICommand<Result<MemberDto>>;

public record RemoveMemberCommand(
    Guid MembershipId
) : ICommand<Result>;
