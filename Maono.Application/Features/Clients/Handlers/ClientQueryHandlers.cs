using Maono.Application.Common.Models;
using Maono.Application.Features.Clients.DTOs;
using Maono.Application.Features.Clients.Queries;
using Maono.Domain.Clients.Repository;
using MediatR;

namespace Maono.Application.Features.Clients.Handlers;

public class GetClientByIdHandler : IRequestHandler<GetClientByIdQuery, Result<ClientDetailDto>>
{
    private readonly IClientRepository _repo;

    public GetClientByIdHandler(IClientRepository repo) => _repo = repo;

    public async Task<Result<ClientDetailDto>> Handle(GetClientByIdQuery request, CancellationToken ct)
    {
        var client = await _repo.GetWithContactsAsync(request.Id, ct);
        if (client == null) return Result.Failure<ClientDetailDto>("Client not found", "NOT_FOUND");

        var contacts = client.Contacts.Select(c =>
            new ClientContactDto(c.Id, c.FullName, c.Email, c.Phone, c.Position, c.IsPrimaryApprover)).ToList();

        var brand = client.BrandProfile != null
            ? new BrandProfileDto(client.BrandProfile.Id, client.BrandProfile.BrandTone, client.BrandProfile.Palette, client.BrandProfile.LogoUrl)
            : null;

        return Result.Success(new ClientDetailDto(
            client.Id, client.Name, client.LegalName, client.BillingEmail,
            client.Phone, client.Notes, client.CreatedAtUtc, contacts, brand));
    }
}

public class ListClientsHandler : IRequestHandler<ListClientsQuery, Result<List<ClientDto>>>
{
    private readonly IClientRepository _repo;

    public ListClientsHandler(IClientRepository repo) => _repo = repo;

    public async Task<Result<List<ClientDto>>> Handle(ListClientsQuery request, CancellationToken ct)
    {
        var clients = string.IsNullOrEmpty(request.Search)
            ? await _repo.GetAllAsync(ct)
            : await _repo.SearchByNameAsync(request.Search, ct);

        var dtos = clients.Select(c => new ClientDto(
            c.Id, c.Name, c.LegalName, c.BillingEmail, c.Phone,
            c.Contacts.Count, c.CreatedAtUtc)).ToList();

        return Result.Success(dtos);
    }
}
