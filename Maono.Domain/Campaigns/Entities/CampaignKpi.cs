using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Campaigns.Entities;

public class CampaignKpi : TenantEntity
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TargetValue { get; set; }
    public string? Unit { get; set; }
    public string? Periodicity { get; set; }

    // Navigation
    public Campaign Campaign { get; set; } = null!;
}
