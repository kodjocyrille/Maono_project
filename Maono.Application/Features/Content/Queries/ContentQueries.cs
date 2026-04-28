using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Content.DTOs;

namespace Maono.Application.Features.Content.Queries;

public record GetContentByIdQuery(Guid Id) : IQuery<Result<ContentItemDetailDto>>;
public record ListContentQuery(string? Status = null) : IQuery<Result<List<ContentItemDto>>>;
public record GetContentByDeadlineQuery(DateTime Deadline) : IQuery<Result<List<ContentItemDto>>>;
