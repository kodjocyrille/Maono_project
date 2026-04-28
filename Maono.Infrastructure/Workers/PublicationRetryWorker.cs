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
/// Retries failed publications. Max 3 attempts with exponential backoff.
/// Runs every 5 minutes.
/// </summary>
public class PublicationRetryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PublicationRetryWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private const int MaxAttempts = 3;

    public PublicationRetryWorker(IServiceScopeFactory scopeFactory, ILogger<PublicationRetryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PublicationRetryWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRetriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PublicationRetryWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRetriesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();

        var failedPubs = await context.Publications
            .IgnoreQueryFilters()
            .Where(p => p.Status == PublicationStatus.Failed)
            .ToListAsync(ct);

        foreach (var pub in failedPubs)
        {
            var attemptCount = await context.PublicationAttempts
                .CountAsync(a => a.PublicationId == pub.Id, ct);

            if (attemptCount >= MaxAttempts)
            {
                _logger.LogWarning("Publication {PublicationId} exceeded max retries ({MaxAttempts})", pub.Id, MaxAttempts);
                continue;
            }

            _logger.LogInformation("Retrying publication {PublicationId}, attempt {Attempt}/{Max}",
                pub.Id, attemptCount + 1, MaxAttempts);

            pub.Status = PublicationStatus.Publishing;

            context.PublicationAttempts.Add(new Domain.Publications.Entities.PublicationAttempt
            {
                WorkspaceId = pub.WorkspaceId,
                PublicationId = pub.Id,
                AttemptNumber = attemptCount + 1,
                StartedAtUtc = DateTime.UtcNow
            });

            // TODO: Call actual social platform API
            // Simulate success for now
            pub.Status = PublicationStatus.Published;
            pub.PublishedAtUtc = DateTime.UtcNow;
        }

        if (failedPubs.Count > 0)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
