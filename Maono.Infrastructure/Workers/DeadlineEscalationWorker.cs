using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maono.Infrastructure.Workers;

/// <summary>
/// Escalates content items approaching deadlines at J-7, J-3, J-1, J-0.
/// Runs every hour.
/// </summary>
public class DeadlineEscalationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DeadlineEscalationWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public DeadlineEscalationWorker(IServiceScopeFactory scopeFactory, ILogger<DeadlineEscalationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeadlineEscalationWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEscalationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeadlineEscalationWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessEscalationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();

        var now = DateTime.UtcNow;

        // ECR-015 — Escalation thresholds with priority levels
        var escalationRules = new (int Days, string Priority, string EscalationType)[]
        {
            (7, "low",      "assignee"),      // J-7: notify assignee only
            (3, "medium",   "assignee+lead"), // J-3: notify assignee + lead
            (1, "high",     "workspace"),     // J-1: notify all workspace members
            (0, "critical", "workspace"),     // J-0: red alert
        };

        foreach (var (days, priority, escalationType) in escalationRules)
        {
            var targetDate = now.AddDays(days).Date;
            var nextDay = targetDate.AddDays(1);

            var contentItems = await context.ContentItems
                .IgnoreQueryFilters()
                .Where(c => c.Deadline.HasValue
                    && c.Deadline.Value >= targetDate
                    && c.Deadline.Value < nextDay
                    && c.Status != ContentStatus.Published
                    && c.Status != ContentStatus.Archived
                    && !c.IsDeleted)
                .ToListAsync(ct);

            if (contentItems.Count == 0) continue;

            _logger.LogInformation("Deadline escalation J-{Days} ({Priority}): {Count} content items",
                days, priority, contentItems.Count);

            foreach (var item in contentItems)
            {
                // ECR-015 — Resolve actual assigned users via ContentTasks
                var assignedUserIds = await context.ContentTasks
                    .IgnoreQueryFilters()
                    .Where(t => t.ContentItemId == item.Id && t.AssignedToUserId.HasValue)
                    .Select(t => t.AssignedToUserId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                // If no assigned users, fall back to workspace-level notification
                if (assignedUserIds.Count == 0)
                    assignedUserIds.Add(Guid.Empty);

                foreach (var userId in assignedUserIds)
                {
                    context.Notifications.Add(new Domain.Notifications.Entities.Notification
                    {
                        WorkspaceId = item.WorkspaceId,
                        UserId = userId,
                        Type = $"deadline.j{days}",
                        Subject = $"[{priority.ToUpper()}] Deadline J-{days} : {item.Title}",
                        Body = $"Le contenu \"{item.Title}\" arrive à échéance dans {days} jour(s). Priorité : {priority}.",
                        Channel = Domain.Notifications.Enums.NotificationChannel.InApp
                    });
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
