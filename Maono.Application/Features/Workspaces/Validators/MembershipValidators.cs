using FluentValidation;
using Maono.Application.Features.Workspaces.Commands;

namespace Maono.Application.Features.Workspaces.Validators;

/// <summary>
/// Rôles système définis dans DatabaseSeeder.cs :
/// Admin, Strategist, Planner, Designer, ClientProxy, FreelancerOwner
/// </summary>
public class InviteMemberValidator : AbstractValidator<InviteMemberCommand>
{
    // Admin exclu — réservé au bootstrap, ne peut pas être assigné par invitation
    private static readonly string[] AllowedRoles =
    [
        "Strategist", "Planner", "Designer", "ClientProxy", "FreelancerOwner"
    ];

    public InviteMemberValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Rôle invalide. Valeurs acceptées : {string.Join(", ", AllowedRoles)}");
    }
}

public class UpdateMemberRoleValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin", "Strategist", "Planner", "Designer", "ClientProxy", "FreelancerOwner"
    ];

    public UpdateMemberRoleValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.NewRoleName)
            .NotEmpty()
            .Must(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Rôle invalide. Valeurs acceptées : {string.Join(", ", AllowedRoles)}");
    }
}
