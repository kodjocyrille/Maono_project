using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Clients.DTOs;

namespace Maono.Application.Features.Clients.Commands;

public record CreateClientCommand(
    string Name,
    string? LegalName,
    string? BillingEmail,
    string? Phone,
    string? Notes
) : ICommand<Result<ClientDto>>;

public record UpdateClientCommand(
    Guid Id,
    string Name,
    string? LegalName,
    string? BillingEmail,
    string? Phone,
    string? Notes
) : ICommand<Result<ClientDto>>;

public record DeleteClientCommand(Guid Id) : ICommand<Result>;
