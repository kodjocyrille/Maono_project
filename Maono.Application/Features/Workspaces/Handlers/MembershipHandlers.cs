using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.Commands;
using Maono.Application.Features.Workspaces.DTOs;
using Maono.Application.Features.Workspaces.Queries;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Identity.Repository;
using MediatR;

namespace Maono.Application.Features.Workspaces.Handlers;

public class ListMembersHandler : IRequestHandler<ListMembersQuery, Result<List<MemberDto>>>
{
    private readonly IMembershipRepository _repo;
    public ListMembersHandler(IMembershipRepository repo) => _repo = repo;

    public async Task<Result<List<MemberDto>>> Handle(ListMembersQuery request, CancellationToken ct)
    {
        var memberships = await _repo.GetByWorkspaceAsync(request.WorkspaceId, ct);
        var dtos = memberships
            .Where(m => m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Invited)
            .Select(m => new MemberDto(
                m.Id, m.UserId,
                m.User?.DisplayName ?? "",
                m.User?.Email ?? "",
                m.Role?.Name ?? "",
                m.Status.ToString(),
                m.JoinedAtUtc))
            .ToList();
        return Result.Success(dtos);
    }
}

public class InviteMemberHandler : IRequestHandler<InviteMemberCommand, Result<InviteMemberResultDto>>
{
    private readonly IMembershipRepository _membershipRepo;
    private readonly IUserRepository _userRepo;
    private readonly IWorkspaceRepository _workspaceRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IInviteTokenService _inviteTokenService;

    public InviteMemberHandler(
        IMembershipRepository membershipRepo,
        IUserRepository userRepo,
        IWorkspaceRepository workspaceRepo,
        IRoleRepository roleRepo,
        IInviteTokenService inviteTokenService)
    {
        _membershipRepo = membershipRepo;
        _userRepo = userRepo;
        _workspaceRepo = workspaceRepo;
        _roleRepo = roleRepo;
        _inviteTokenService = inviteTokenService;
    }

    public async Task<Result<InviteMemberResultDto>> Handle(InviteMemberCommand request, CancellationToken ct)
    {
        // 1. Résoudre l'utilisateur
        var user = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (user == null)
            return Result.Failure<InviteMemberResultDto>("Utilisateur introuvable avec cet email.", "USER_NOT_FOUND");

        // 2. Vérifier s'il est déjà membre
        var existing = await _membershipRepo.GetByUserAndWorkspaceAsync(user.Id, request.WorkspaceId, ct);
        if (existing != null && existing.Status == MembershipStatus.Active)
            return Result.Failure<InviteMemberResultDto>("Cet utilisateur est déjà membre du workspace.", "ALREADY_MEMBER");
        if (existing != null && existing.Status == MembershipStatus.Invited)
            return Result.Failure<InviteMemberResultDto>("Cet utilisateur a déjà une invitation en attente.", "ALREADY_INVITED");

        // 3. Résoudre le rôle par son nom
        var role = await _roleRepo.GetByNameAsync(request.RoleName, ct);
        if (role == null)
            return Result.Failure<InviteMemberResultDto>($"Rôle introuvable : '{request.RoleName}'. Vérifiez les rôles disponibles via GET /api/admin/roles.", "ROLE_NOT_FOUND");

        // 4. Résoudre le workspace pour le token
        var workspace = await _workspaceRepo.GetByIdAsync(request.WorkspaceId, ct);
        if (workspace == null)
            return Result.Failure<InviteMemberResultDto>("Workspace introuvable.", "WORKSPACE_NOT_FOUND");

        // 5. Créer la membership avec statut Invited (pas Active)
        var membership = new WorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = request.WorkspaceId,
            RoleId = role.Id,
            Status = MembershipStatus.Invited,
            JoinedAtUtc = DateTime.UtcNow
        };
        await _membershipRepo.AddAsync(membership, ct);

        // 6. Générer le token d'invitation JWT
        var inviteToken = await _inviteTokenService.GenerateTokenAsync(
            workspace.Id, workspace.Name, user.Email, role.Name, ct);

        var memberDto = new MemberDto(
            membership.Id, user.Id, user.DisplayName, user.Email,
            role.Name, MembershipStatus.Invited.ToString(), membership.JoinedAtUtc);

        return Result.Success(new InviteMemberResultDto(memberDto, inviteToken));
    }
}

// ── Accepter l'invitation ──────────────────────────────────────────────

public class AcceptInvitationHandler : IRequestHandler<AcceptInvitationCommand, Result<MemberDto>>
{
    private readonly IMembershipRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public AcceptInvitationHandler(IMembershipRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<MemberDto>> Handle(AcceptInvitationCommand request, CancellationToken ct)
    {
        var membership = await _repo.GetByIdWithNavigationsAsync(request.MembershipId, ct);
        if (membership == null)
            return Result.Failure<MemberDto>("Invitation introuvable.", "NOT_FOUND");

        if (membership.Status != MembershipStatus.Invited)
            return Result.Failure<MemberDto>("Cette invitation a déjà été traitée.", "INVALID_STATUS");

        // Vérifier que l'utilisateur courant est bien le destinataire
        if (_currentUser.UserId != membership.UserId)
            return Result.Failure<MemberDto>("Vous n'êtes pas le destinataire de cette invitation.", "FORBIDDEN");

        // Activer la membership
        membership.Status = MembershipStatus.Active;
        membership.JoinedAtUtc = DateTime.UtcNow;
        _repo.Update(membership);

        return Result.Success(new MemberDto(
            membership.Id, membership.UserId,
            membership.User?.DisplayName ?? "",
            membership.User?.Email ?? "",
            membership.Role?.Name ?? "",
            MembershipStatus.Active.ToString(),
            membership.JoinedAtUtc));
    }
}

// ── Retirer un membre ──────────────────────────────────────────────────

public class RemoveMemberHandler : IRequestHandler<RemoveMemberCommand, Result>
{
    private readonly IMembershipRepository _repo;
    public RemoveMemberHandler(IMembershipRepository repo) => _repo = repo;

    public async Task<Result> Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var membership = await _repo.GetByIdAsync(request.MembershipId, ct);
        if (membership == null)
            return Result.Failure("Membership introuvable.", "NOT_FOUND");

        var isLast = await _repo.IsLastAdminAsync(membership.WorkspaceId, membership.Id, ct);
        if (isLast)
            return Result.Failure("Impossible de retirer le dernier administrateur du workspace.", "LAST_ADMIN");

        membership.Status = MembershipStatus.Removed;
        _repo.Update(membership);
        return Result.Success();
    }
}

// ── Modifier le rôle d'un membre ───────────────────────────────────────

public class UpdateMemberRoleHandler : IRequestHandler<UpdateMemberRoleCommand, Result<MemberDto>>
{
    private readonly IMembershipRepository _repo;
    private readonly IRoleRepository _roleRepo;

    public UpdateMemberRoleHandler(IMembershipRepository repo, IRoleRepository roleRepo)
    {
        _repo = repo;
        _roleRepo = roleRepo;
    }

    public async Task<Result<MemberDto>> Handle(UpdateMemberRoleCommand request, CancellationToken ct)
    {
        var membership = await _repo.GetByIdWithNavigationsAsync(request.MembershipId, ct);
        if (membership == null)
            return Result.Failure<MemberDto>("Membership introuvable.", "NOT_FOUND");

        var isLast = await _repo.IsLastAdminAsync(membership.WorkspaceId, membership.Id, ct);
        if (isLast && request.NewRoleName.ToUpper() != "ADMIN")
            return Result.Failure<MemberDto>("Impossible de rétrograder le dernier administrateur.", "LAST_ADMIN");

        // Résoudre le nouveau rôle par nom
        var newRole = await _roleRepo.GetByNameAsync(request.NewRoleName, ct);
        if (newRole == null)
            return Result.Failure<MemberDto>($"Rôle introuvable : '{request.NewRoleName}'.", "ROLE_NOT_FOUND");

        // Mettre à jour le RoleId (correction du bug)
        membership.RoleId = newRole.Id;
        _repo.Update(membership);

        return Result.Success(new MemberDto(
            membership.Id, membership.UserId,
            membership.User?.DisplayName ?? "",
            membership.User?.Email ?? "",
            newRole.Name,
            membership.Status.ToString(),
            membership.JoinedAtUtc));
    }
}
