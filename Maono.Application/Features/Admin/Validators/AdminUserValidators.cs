using FluentValidation;
using Maono.Application.Features.Admin.Commands;

namespace Maono.Application.Features.Admin.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin", "Strategist", "Planner", "Designer", "ClientProxy", "FreelancerOwner"
    ];

    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Rôle invalide. Valeurs acceptées : {string.Join(", ", AllowedRoles)}");
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin", "Strategist", "Planner", "Designer", "ClientProxy", "FreelancerOwner"
    ];

    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleName)
            .Must(r => r == null || AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Rôle invalide. Valeurs acceptées : {string.Join(", ", AllowedRoles)}");
    }
}
