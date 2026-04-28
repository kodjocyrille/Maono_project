using Maono.Domain.Performance.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Performance.Entities;

public class PerformanceSnapshot : TenantEntity
{
    public Guid? PublicationId { get; set; }
    public Guid? ContentItemId { get; set; }
    public DateTime CollectedAtUtc { get; set; } = DateTime.UtcNow;
    public long Impressions { get; set; }
    public long Reach { get; set; }
    public long Engagement { get; set; }
    public long Clicks { get; set; }
    public decimal? ConversionRate { get; set; }
}
