using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Publications.DTOs;

namespace Maono.Application.Features.Publications.Queries;

public record GetPublicationByIdQuery(Guid Id) : IQuery<Result<PublicationDetailDto>>;
public record ListPublicationsQuery(string? Status = null) : IQuery<Result<List<PublicationDto>>>;
