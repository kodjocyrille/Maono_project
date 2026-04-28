using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Tasks.Commands;
using Maono.Application.Features.Tasks.DTOs;
using Maono.Application.Features.Tasks.Queries;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Tasks.Handlers;

public class GetMyTasksHandler : IRequestHandler<GetMyTasksQuery, Result<List<TaskDto>>>
{
    private readonly IContentTaskRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public GetMyTasksHandler(IContentTaskRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<TaskDto>>> Handle(GetMyTasksQuery request, CancellationToken ct)
    {
        if (_currentUser.UserId == null)
            return Result.Failure<List<TaskDto>>("Utilisateur non authentifié.", "UNAUTHORIZED");

        var tasks = await _repo.GetByAssignedUserAsync(_currentUser.UserId.Value, request.Status, ct);

        var dtos = tasks.Select(t => new TaskDto(
            t.Id, t.ContentItemId, t.Title, t.Description,
            t.Status, t.Priority, t.AssignedToUserId,
            t.DueDate, t.BlockedReason, t.CreatedAtUtc, t.CompletedAtUtc))
            .ToList();

        return Result.Success(dtos);
    }
}

public class ListTasksHandler : IRequestHandler<ListTasksQuery, Result<List<TaskDto>>>
{
    private readonly IContentTaskRepository _repo;
    public ListTasksHandler(IContentTaskRepository repo) => _repo = repo;

    public async Task<Result<List<TaskDto>>> Handle(ListTasksQuery request, CancellationToken ct)
    {
        var tasks = await _repo.GetByContentItemAsync(request.ContentItemId, ct);
        var dtos = tasks
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .Select(t => new TaskDto(
                t.Id, t.ContentItemId, t.Title, t.Description,
                t.Status, t.Priority, t.AssignedToUserId,
                t.DueDate, t.BlockedReason, t.CreatedAtUtc, t.CompletedAtUtc))
            .ToList();
        return Result.Success(dtos);
    }
}

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    private readonly IContentTaskRepository _repo;
    public CreateTaskHandler(IContentTaskRepository repo) => _repo = repo;

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var task = new ContentTask
        {
            ContentItemId = request.ContentItemId,
            Title = request.Title,
            Description = request.Description,
            AssignedToUserId = request.AssignedToUserId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Status = ContentTaskStatus.Pending
        };
        await _repo.AddAsync(task, ct);
        return Result.Success(new TaskDto(
            task.Id, task.ContentItemId, task.Title, task.Description,
            task.Status, task.Priority, task.AssignedToUserId,
            task.DueDate, task.BlockedReason, task.CreatedAtUtc, task.CompletedAtUtc));
    }
}

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, Result<TaskDto>>
{
    private readonly IContentTaskRepository _repo;
    public UpdateTaskHandler(IContentTaskRepository repo) => _repo = repo;

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(request.TaskId, ct);
        if (task == null) return Result.Failure<TaskDto>("Tâche introuvable.", "NOT_FOUND");

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.AssignedToUserId.HasValue) task.AssignedToUserId = request.AssignedToUserId;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate;
        if (request.Priority.HasValue) task.Priority = request.Priority.Value;
        if (request.BlockedReason != null) task.BlockedReason = request.BlockedReason;

        if (request.Status.HasValue)
        {
            var newStatus = request.Status.Value;

            // Prevent completing a blocked task — must unblock first
            if (newStatus == ContentTaskStatus.Completed && task.Status == ContentTaskStatus.Blocked)
            {
                return Result.Failure<TaskDto>(
                    "Impossible de compléter une tâche bloquée. Débloquez-la d'abord en changeant son statut à InProgress.",
                    "TASK_BLOCKED");
            }

            // When unblocking, clear the blocked reason
            if (task.Status == ContentTaskStatus.Blocked && newStatus != ContentTaskStatus.Blocked)
            {
                task.BlockedReason = null;
            }

            task.Status = newStatus;

            if (newStatus == ContentTaskStatus.Completed)
            {
                task.CompletedAtUtc = DateTime.UtcNow;

                // Auto-unblock: check sibling blocked tasks on the same content
                var siblingTasks = await _repo.GetByContentItemAsync(task.ContentItemId, ct);
                var blockedSiblings = siblingTasks
                    .Where(t => t.Id != task.Id && t.Status == ContentTaskStatus.Blocked)
                    .ToList();

                // Check if remaining non-completed tasks (excluding blocked) exist
                var remainingIncomplete = siblingTasks
                    .Where(t => t.Id != task.Id
                        && t.Status != ContentTaskStatus.Completed
                        && t.Status != ContentTaskStatus.Blocked)
                    .ToList();

                // If no more active tasks are blocking, unblock the blocked siblings
                if (remainingIncomplete.Count == 0)
                {
                    foreach (var blocked in blockedSiblings)
                    {
                        blocked.Status = ContentTaskStatus.Pending;
                        blocked.BlockedReason = null;
                        _repo.Update(blocked);
                    }
                }
            }
        }

        _repo.Update(task);
        return Result.Success(new TaskDto(
            task.Id, task.ContentItemId, task.Title, task.Description,
            task.Status, task.Priority, task.AssignedToUserId,
            task.DueDate, task.BlockedReason, task.CreatedAtUtc, task.CompletedAtUtc));
    }
}

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IContentTaskRepository _repo;
    public DeleteTaskHandler(IContentTaskRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(request.TaskId, ct);
        if (task == null) return Result.Failure("Tâche introuvable.", "NOT_FOUND");
        _repo.Remove(task);
        return Result.Success();
    }
}
