using FluentValidation;
using Maono.Application.Features.Admin.Commands;

namespace Maono.Application.Features.Admin.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du rôle est requis.")
            .MaximumLength(50).WithMessage("Le nom du rôle ne peut pas dépasser 50 caractères.")
            .Matches("^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Le nom du rôle doit commencer par une lettre et ne contenir que des lettres, chiffres et underscores.");

        RuleFor(x => x.PermissionCodes)
            .NotEmpty().WithMessage("Au moins une permission est requise.");
    }
}

public class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("L'identifiant du rôle est requis.");
        RuleFor(x => x.PermissionCodes)
            .NotEmpty().WithMessage("Au moins une permission est requise.");
    }
}

public class AssignUserRoleValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("L'identifiant de l'utilisateur est requis.");
        RuleFor(x => x.WorkspaceId).NotEmpty().WithMessage("L'identifiant du workspace est requis.");
        RuleFor(x => x.RoleName).NotEmpty().WithMessage("Le nom du rôle est requis.");
    }
}
