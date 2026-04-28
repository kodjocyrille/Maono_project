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
/// Publishes content scheduled for the current time window.
/// Runs every minute.
/// </summary>
public class ScheduledPublicationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledPublicationWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public ScheduledPublicationWorker(IServiceScopeFactory scopeFactory, ILogger<ScheduledPublicationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledPublicationWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledPublicationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ScheduledPublicationWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessScheduledPublicationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();

        var now = DateTime.UtcNow;

        var scheduledPubs = await context.Publications
            .IgnoreQueryFilters()
            .Where(p => p.Status == PublicationStatus.Scheduled
                && p.ScheduledAtUtc.HasValue
                && p.ScheduledAtUtc.Value <= now)
            .ToListAsync(ct);

        foreach (var pub in scheduledPubs)
        {
            _logger.LogInformation("Publishing scheduled content: Publication {PublicationId} on {Platform}",
                pub.Id, pub.Platform);

            pub.Status = PublicationStatus.Publishing;

            context.PublicationAttempts.Add(new Domain.Publications.Entities.PublicationAttempt
            {
                WorkspaceId = pub.WorkspaceId,
                PublicationId = pub.Id,
                AttemptNumber = 1,
                StartedAtUtc = now
            });

            // TODO: Call actual social platform API here
            // For now, mark as published after "attempt"
            pub.Status = PublicationStatus.Published;
            pub.PublishedAtUtc = now;

            _logger.LogInformation("Publication {PublicationId} marked as published", pub.Id);
        }

        if (scheduledPubs.Count > 0)
        {
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Processed {Count} scheduled publications", scheduledPubs.Count);
        }
    }
}
