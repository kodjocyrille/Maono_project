using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Tasks.DTOs;
using Maono.Domain.Content.Entities;

namespace Maono.Application.Features.Tasks.Commands;

public record CreateTaskCommand(
    Guid ContentItemId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    ContentTaskPriority Priority,
    DateTime? DueDate
) : ICommand<Result<TaskDto>>;

public record UpdateTaskCommand(
    Guid TaskId,
    string? Title,
    string? Description,
    ContentTaskStatus? Status,
    ContentTaskPriority? Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    string? BlockedReason
) : ICommand<Result<TaskDto>>;

public record DeleteTaskCommand(Guid TaskId) : ICommand<Result>;
