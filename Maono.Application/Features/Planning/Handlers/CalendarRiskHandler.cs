using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.Queries;
using Maono.Domain.Common;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Enums;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Planning.Handlers;

/// <summary>
/// ECR-021 — Computes calendar risk indicators:
/// - Overdue contents (deadline passed, not Published/Archived)
/// - Pending items (in-review/client-review for too long)
/// </summary>
public class GetCalendarRisksHandler : IRequestHandler<GetCalendarRisksQuery, Result<CalendarRiskReport>>
{
    private readonly IContentRepository _contentRepo;
    private readonly IGenericRepository<ContentTask> _taskRepo;

    public GetCalendarRisksHandler(IContentRepository contentRepo, IGenericRepository<ContentTask> taskRepo)
    {
        _contentRepo = contentRepo;
        _taskRepo = taskRepo;
    }

    public async Task<Result<CalendarRiskReport>> Handle(GetCalendarRisksQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var risks = new List<RiskItem>();

        // 1. Overdue contents — deadline passed but not Published/Archived
        var overdue = await _contentRepo.FindAsync(c =>
            c.Deadline.HasValue &&
            c.Deadline.Value < now &&
            c.Status != ContentStatus.Published &&
            c.Status != ContentStatus.Archived, ct);

        foreach (var item in overdue)
        {
            risks.Add(new RiskItem(
                "OVERDUE", item.Id, item.Title,
                $"En retard de {(now - item.Deadline!.Value).Days} jour(s), statut actuel : {item.Status}",
                item.Deadline));
        }

        // 2. Blocked tasks
        var blockedTasks = await _taskRepo.FindAsync(t =>
            t.Status == ContentTaskStatus.Blocked, ct);

        foreach (var task in blockedTasks)
        {
            risks.Add(new RiskItem(
                "BLOCKED_TASK", task.Id, task.Title,
                $"Tâche bloquée : {task.BlockedReason ?? "raison non spécifiée"}",
                task.DueDate));
        }

        return Result.Success(new CalendarRiskReport(
            OverdueContents: overdue.Count,
            PendingApprovals: 0, // Could be enriched with approval cycle data
            OverloadedResources: 0, // Could be enriched with capacity data
            Items: risks));
    }
}
