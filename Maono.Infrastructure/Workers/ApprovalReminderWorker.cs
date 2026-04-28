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
/// Sends reminder notifications for pending approval cycles after X days.
/// Runs every 6 hours.
/// </summary>
public class ApprovalReminderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApprovalReminderWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private const int ReminderAfterDays = 3;

    public ApprovalReminderWorker(IServiceScopeFactory scopeFactory, ILogger<ApprovalReminderWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ApprovalReminderWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovalReminderWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();

        var now = DateTime.UtcNow;

        // ECR-016 — Find pending approval cycles (not completed)
        var pendingCycles = await context.ApprovalCycles
            .IgnoreQueryFilters()
            .Where(c => (c.InternalStatus == ApprovalStatus.Pending || c.ClientStatus == ApprovalStatus.Pending)
                && c.CompletedAtUtc == null)
            .Include(c => c.ContentItem)
            .ToListAsync(ct);

        if (pendingCycles.Count == 0) return;

        _logger.LogInformation("Approval reminder: processing {Count} pending cycles", pendingCycles.Count);

        foreach (var cycle in pendingCycles)
        {
            var contentTitle = cycle.ContentItem?.Title ?? "Unknown";

            // Determine reminder level based on deadline or age
            string reminderType;
            string priority;

            if (cycle.DeadlineUtc.HasValue)
            {
                // ECR-016 — Deadline-based reminders
                var daysUntilDeadline = (cycle.DeadlineUtc.Value - now).TotalDays;

                if (daysUntilDeadline <= 0)
                    { reminderType = "overdue"; priority = "CRITICAL"; }
                else if (daysUntilDeadline <= 1)
                    { reminderType = "j1"; priority = "HIGH"; }
                else if (daysUntilDeadline <= 3)
                    { reminderType = "j3"; priority = "MEDIUM"; }
                else
                    continue; // Not yet due for a reminder
            }
            else
            {
                // Fallback: time-since-start based reminders
                var daysSinceStart = (now - cycle.StartedAtUtc).TotalDays;
                if (daysSinceStart < ReminderAfterDays) continue;
                reminderType = "age"; priority = "MEDIUM";
            }

            // Avoid duplicate reminders: check if we already sent for this level
            var expectedCount = reminderType switch
            {
                "overdue" => 3, "j1" => 2, "j3" => 1, _ => 1
            };
            if (cycle.ReminderSentCount >= expectedCount) continue;

            if (cycle.InternalStatus == ApprovalStatus.Pending)
            {
                context.Notifications.Add(new Domain.Notifications.Entities.Notification
                {
                    WorkspaceId = cycle.WorkspaceId,
                    UserId = Guid.Empty,
                    Type = $"approval.reminder.internal.{reminderType}",
                    Subject = $"[{priority}] Rappel : Approbation interne en attente",
                    Body = $"L'approbation interne pour \"{contentTitle}\" nécessite votre attention. " +
                           (cycle.DeadlineUtc.HasValue
                               ? $"Deadline : {cycle.DeadlineUtc.Value:dd/MM/yyyy}."
                               : $"En attente depuis {(now - cycle.StartedAtUtc).Days} jour(s)."),
                    Channel = NotificationChannel.InApp
                });
            }

            if (cycle.ClientStatus == ApprovalStatus.Pending)
            {
                context.Notifications.Add(new Domain.Notifications.Entities.Notification
                {
                    WorkspaceId = cycle.WorkspaceId,
                    UserId = Guid.Empty,
                    Type = $"approval.reminder.client.{reminderType}",
                    Subject = $"[{priority}] Rappel : Approbation client en attente",
                    Body = $"L'approbation client pour \"{contentTitle}\" nécessite votre attention. " +
                           (cycle.DeadlineUtc.HasValue
                               ? $"Deadline : {cycle.DeadlineUtc.Value:dd/MM/yyyy}."
                               : $"En attente depuis {(now - cycle.StartedAtUtc).Days} jour(s)."),
                    Channel = NotificationChannel.Email
                });
            }

            // Track reminder count
            cycle.ReminderSentCount = expectedCount;
        }

        await context.SaveChangesAsync(ct);
    }
}

