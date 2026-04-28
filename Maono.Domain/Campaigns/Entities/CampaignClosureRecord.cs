using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Campaigns.Entities;

public class CampaignClosureRecord : TenantEntity
{
    public Guid CampaignId { get; set; }
    public DateTime ClosedAtUtc { get; set; }
    public string? ClosedBy { get; set; }
    public string? Summary { get; set; }

    // Navigation
    public Campaign Campaign { get; set; } = null!;
}
