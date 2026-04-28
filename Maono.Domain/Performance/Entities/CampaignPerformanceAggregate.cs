using Maono.Domain.Performance.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Performance.Entities;

/// <summary>
/// Materialized/aggregated campaign performance for dashboards.
/// </summary>
public class CampaignPerformanceAggregate : TenantEntity
{
    public Guid CampaignId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public long TotalImpressions { get; set; }
    public long TotalReach { get; set; }
    public long TotalEngagement { get; set; }
    public long TotalClicks { get; set; }
    public decimal? AverageConversionRate { get; set; }
    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
