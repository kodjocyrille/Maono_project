using FluentValidation;
using Maono.Application.Features.Clients.Commands;

namespace Maono.Application.Features.Clients.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BillingEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.BillingEmail));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class UpdateClientValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BillingEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.BillingEmail));
    }
}

public class DeleteClientValidator : AbstractValidator<DeleteClientCommand>
{
    public DeleteClientValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
