using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Commands;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using MediatR;

namespace Maono.Application.Features.Auth.Handlers;

// ══════════════════════════════════════════════════════════════
// 1. Self-Register — Public (Freelancer / Agency Owner)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Creates User + Workspace + Membership(FreelancerOwner).
/// The user becomes the owner of their own workspace.
/// </summary>
public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;
    private readonly IWorkspaceRepository _workspaceRepo;

    public RegisterHandler(
        IAuthenticationService authService,
        IRoleRepository roleRepo,
        IMembershipRepository membershipRepo,
        IWorkspaceRepository workspaceRepo)
    {
        _authService = authService;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
        _workspaceRepo = workspaceRepo;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        // 1. Create user
        var userResult = await _authService.RegisterAsync(
            request.Email, request.Password,
            request.FirstName, request.LastName, request.PhoneNumber,
            ct: ct);

        if (!userResult.IsSuccess)
            return Result.Failure<RegisterResponse>(userResult.Error!, "REGISTRATION_FAILED");

        var user = userResult.Value!;

        // 2. Create workspace
        var slug = GenerateSlug(request.WorkspaceName);
        var workspace = new Workspace
        {
            Name = request.WorkspaceName,
            Slug = slug,
        };
        await _workspaceRepo.AddAsync(workspace, ct);

        // 3. Assign FreelancerOwner role
        var role = await _roleRepo.GetByNameAsync("FreelancerOwner", ct)
                   ?? await _roleRepo.GetByNameAsync("Admin", ct); // fallback

        var membership = new WorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id,
            RoleId = role!.Id,
            IsDefault = true,
        };
        await _membershipRepo.AddAsync(membership, ct);

        return Result.Success(new RegisterResponse(
            user.Id, user.Email, user.DisplayName,
            workspace.Id, workspace.Name, role.Name));
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
        // Remove non-alphanumeric except hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Append random suffix for uniqueness
        slug += $"-{Guid.NewGuid().ToString("N")[..6]}";
        return slug;
    }
}

// ══════════════════════════════════════════════════════════════
// 2. Register by Invitation
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Creates User + Membership in an existing workspace.
/// The invite token contains workspace ID, role, and target email.
/// </summary>
public class RegisterByInviteHandler : IRequestHandler<RegisterByInviteCommand, Result<RegisterResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;
    private readonly IInviteTokenService _inviteTokenService;

    public RegisterByInviteHandler(
        IAuthenticationService authService,
        IRoleRepository roleRepo,
        IMembershipRepository membershipRepo,
        IInviteTokenService inviteTokenService)
    {
        _authService = authService;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
        _inviteTokenService = inviteTokenService;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterByInviteCommand request, CancellationToken ct)
    {
        // 1. Validate invite token
        var invite = await _inviteTokenService.ValidateTokenAsync(request.InviteToken, ct);
        if (invite == null)
            return Result.Failure<RegisterResponse>("Lien d'invitation invalide ou expiré.", "INVALID_INVITE");

        // 2. Create user
        var userResult = await _authService.RegisterAsync(
            invite.Email, request.Password,
            request.FirstName, request.LastName, request.PhoneNumber,
            ct: ct);

        if (!userResult.IsSuccess)
            return Result.Failure<RegisterResponse>(userResult.Error!, "REGISTRATION_FAILED");

        var user = userResult.Value!;

        // 3. Resolve role
        var role = await _roleRepo.GetByNameAsync(invite.RoleName, ct);
        if (role == null)
            return Result.Failure<RegisterResponse>($"Rôle '{invite.RoleName}' introuvable.", "ROLE_NOT_FOUND");

        // 4. Create membership in existing workspace
        var membership = new WorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = invite.WorkspaceId,
            RoleId = role.Id,
            IsDefault = true,
        };
        await _membershipRepo.AddAsync(membership, ct);

        // 5. Mark invite as used
        await _inviteTokenService.MarkAsUsedAsync(request.InviteToken, ct);

        return Result.Success(new RegisterResponse(
            user.Id, user.Email, user.DisplayName,
            invite.WorkspaceId, invite.WorkspaceName, role.Name));
    }
}

// ══════════════════════════════════════════════════════════════
// 3. Admin Creates User
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Admin creates a user and assigns them to a specific workspace with a role.
/// </summary>
public class AdminCreateUserHandler : IRequestHandler<AdminCreateUserCommand, Result<RegisterResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;
    private readonly IWorkspaceRepository _workspaceRepo;

    public AdminCreateUserHandler(
        IAuthenticationService authService,
        IRoleRepository roleRepo,
        IMembershipRepository membershipRepo,
        IWorkspaceRepository workspaceRepo)
    {
        _authService = authService;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
        _workspaceRepo = workspaceRepo;
    }

    public async Task<Result<RegisterResponse>> Handle(AdminCreateUserCommand request, CancellationToken ct)
    {
        // 1. Validate workspace
        var workspace = await _workspaceRepo.GetByIdAsync(request.WorkspaceId, ct);
        if (workspace == null)
            return Result.Failure<RegisterResponse>("Workspace introuvable.", "WORKSPACE_NOT_FOUND");

        // 2. Validate role
        var role = await _roleRepo.GetByNameAsync(request.RoleName, ct);
        if (role == null)
            return Result.Failure<RegisterResponse>($"Rôle '{request.RoleName}' introuvable.", "ROLE_NOT_FOUND");

        // 3. Create user
        var password = request.Password ?? $"Temp@{Guid.NewGuid().ToString("N")[..8]}!";
        var userResult = await _authService.RegisterAsync(
            request.Email, password,
            request.FirstName, request.LastName, request.PhoneNumber,
            ct: ct);

        if (!userResult.IsSuccess)
            return Result.Failure<RegisterResponse>(userResult.Error!, "REGISTRATION_FAILED");

        var user = userResult.Value!;

        // 4. Create membership
        var membership = new WorkspaceMembership
        {
            UserId = user.Id,
            WorkspaceId = workspace.Id,
            RoleId = role.Id,
            IsDefault = true,
        };
        await _membershipRepo.AddAsync(membership, ct);

        return Result.Success(new RegisterResponse(
            user.Id, user.Email, user.DisplayName,
            workspace.Id, workspace.Name, role.Name));
    }
}
