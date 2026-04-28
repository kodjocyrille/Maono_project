using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Clients.DTOs;

namespace Maono.Application.Features.Clients.Queries;

public record GetClientByIdQuery(Guid Id) : IQuery<Result<ClientDetailDto>>;
public record ListClientsQuery(string? Search = null) : IQuery<Result<List<ClientDto>>>;
