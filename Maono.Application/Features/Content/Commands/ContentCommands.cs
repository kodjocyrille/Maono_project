using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Content.DTOs;

namespace Maono.Application.Features.Content.Commands;

public record CreateContentCommand(
    string Title,
    string? Format,
    DateTime? Deadline,
    int Priority,
    Guid? CalendarEntryId
) : ICommand<Result<ContentItemDto>>;

public record UpdateContentStatusCommand(
    Guid Id,
    Domain.Content.Enums.ContentStatus NewStatus
) : ICommand<Result<ContentItemDto>>;
