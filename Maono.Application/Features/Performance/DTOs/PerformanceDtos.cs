namespace Maono.Application.Features.Performance.DTOs;

public record PerformanceSnapshotDto(Guid Id, Guid? PublicationId, Guid? ContentItemId, DateTime CollectedAtUtc, long Impressions, long Reach, long Engagement, long Clicks, decimal? ConversionRate);
public record CampaignPerformanceSummaryDto(Guid CampaignId, long TotalImpressions, long TotalReach, long TotalEngagement, long TotalClicks, decimal? AverageConversionRate);
