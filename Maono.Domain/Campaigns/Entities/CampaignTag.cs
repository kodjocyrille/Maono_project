using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Common;
using Maono.Domain.Content.Entities;

namespace Maono.Domain.Campaigns.Entities;

public class CampaignTag : TenantEntity
{
    public Guid CampaignId { get; set; }
    public Guid TagId { get; set; }

    // Navigation
    public Campaign Campaign { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
