using Maono.Domain.Content.Enums;

namespace Maono.Application.Features.Content.DTOs;

public record ContentItemDto(
    Guid Id,
    string Title,
    string? Format,
    ContentStatus Status,
    DateTime? Deadline,
    int Priority,
    int CurrentRevisionNumber,
    DateTime CreatedAtUtc
);

public record ContentItemDetailDto(
    Guid Id,
    string Title,
    string? Format,
    ContentStatus Status,
    DateTime? Deadline,
    int Priority,
    int CurrentRevisionNumber,
    Guid? CalendarEntryId,
    DateTime CreatedAtUtc,
    List<BriefDto> Briefs,
    List<ChecklistItemDto> ChecklistItems
);

public record BriefDto(Guid Id, string Body, DateTime CreatedAtUtc);
public record ChecklistItemDto(Guid Id, string Label, bool IsCompleted);
