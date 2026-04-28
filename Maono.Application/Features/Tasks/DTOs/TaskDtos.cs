using Maono.Domain.Content.Entities;

namespace Maono.Application.Features.Tasks.DTOs;

public record TaskDto(
    Guid Id,
    Guid ContentItemId,
    string Title,
    string? Description,
    ContentTaskStatus Status,
    ContentTaskPriority Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    string? BlockedReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);
