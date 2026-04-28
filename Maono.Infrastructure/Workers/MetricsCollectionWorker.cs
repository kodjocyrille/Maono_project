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
/// Collects performance metrics from social platforms periodically.
/// Runs every 4 hours.
/// </summary>
public class MetricsCollectionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsCollectionWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(4);

    public MetricsCollectionWorker(IServiceScopeFactory scopeFactory, ILogger<MetricsCollectionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MetricsCollectionWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectMetricsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MetricsCollectionWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CollectMetricsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();

        var publishedPubs = await context.Publications
            .IgnoreQueryFilters()
            .Where(p => p.Status == PublicationStatus.Published && p.ExternalPostId != null)
            .ToListAsync(ct);

        _logger.LogInformation("Collecting metrics for {Count} published posts", publishedPubs.Count);

        foreach (var pub in publishedPubs)
        {
            // TODO: Call platform APIs (Instagram, Facebook, LinkedIn, etc.) to fetch metrics
            // For now, create placeholder snapshots

            context.PerformanceSnapshots.Add(new Domain.Performance.Entities.PerformanceSnapshot
            {
                WorkspaceId = pub.WorkspaceId,
                PublicationId = pub.Id,
                ContentItemId = pub.ContentItemId,
                CollectedAtUtc = DateTime.UtcNow,
                // Metrics would come from the platform API
                Impressions = 0,
                Reach = 0,
                Engagement = 0,
                Clicks = 0
            });
        }

        if (publishedPubs.Count > 0)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
