using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Auth.Commands;

// ── 1. Self-Register (Freelancer/Agency owner) ──────────────
/// <summary>
/// Public self-registration. Creates User + Workspace + Membership(FreelancerOwner).
/// Used by freelancers and agency owners creating their own workspace.
/// </summary>
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string WorkspaceName
) : ICommand<Result<RegisterResponse>>;

// ── 2. Register via Invitation ──────────────────────────────
/// <summary>
/// Invitation-based registration. Creates User + Membership in existing workspace.
/// The invite token contains workspace ID and role.
/// </summary>
public record RegisterByInviteCommand(
    string InviteToken,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<Result<RegisterResponse>>;

// ── 3. Admin creates user ──────────────────────────────────
/// <summary>
/// Admin creates a user and assigns them to a workspace with a role.
/// </summary>
public record AdminCreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Password,
    Guid WorkspaceId,
    string RoleName
) : ICommand<Result<RegisterResponse>>;

// ── Response ────────────────────────────────────────────────
public record RegisterResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid? WorkspaceId,
    string? WorkspaceName,
    string? RoleName
);
