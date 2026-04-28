using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Clients.Commands;
using Maono.Application.Features.Clients.DTOs;
using Maono.Domain.Clients.Entities;
using Maono.Domain.Clients.Repository;
using MediatR;

namespace Maono.Application.Features.Clients.Handlers;

public class CreateClientHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
{
    private readonly IClientRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public CreateClientHandler(IClientRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken ct)
    {
        var client = new ClientOrganization
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Name = request.Name,
            LegalName = request.LegalName,
            BillingEmail = request.BillingEmail,
            Phone = request.Phone,
            Notes = request.Notes
        };
        await _repo.AddAsync(client, ct);

        return Result.Success(new ClientDto(
            client.Id, client.Name, client.LegalName, client.BillingEmail,
            client.Phone, 0, client.CreatedAtUtc));
    }
}

public class UpdateClientHandler : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
{
    private readonly IClientRepository _repo;

    public UpdateClientHandler(IClientRepository repo) => _repo = repo;

    public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(request.Id, ct);
        if (client == null) return Result.Failure<ClientDto>("Client not found", "NOT_FOUND");

        client.Name = request.Name;
        client.LegalName = request.LegalName;
        client.BillingEmail = request.BillingEmail;
        client.Phone = request.Phone;
        client.Notes = request.Notes;
        _repo.Update(client);

        return Result.Success(new ClientDto(
            client.Id, client.Name, client.LegalName, client.BillingEmail,
            client.Phone, 0, client.CreatedAtUtc));
    }
}

public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, Result>
{
    private readonly IClientRepository _repo;

    public DeleteClientHandler(IClientRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteClientCommand request, CancellationToken ct)
    {
        var client = await _repo.GetByIdAsync(request.Id, ct);
        if (client == null) return Result.Failure("Client not found", "NOT_FOUND");
        _repo.Remove(client);
        return Result.Success();
    }
}
