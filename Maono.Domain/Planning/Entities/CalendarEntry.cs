using Maono.Domain.Notifications.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Planning.Entities;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;

namespace Maono.Domain.Planning.Entities;

/// <summary>
/// Calendar entry: a planned slot in the editorial calendar.
/// </summary>
public class CalendarEntry : TenantEntity
{
    public Guid CampaignId { get; set; }
    public DateTime PublicationDate { get; set; }
    public SocialPlatform Platform { get; set; }
    public string? ContentType { get; set; }
    public string? Theme { get; set; }
    public string? Status { get; set; }

    // Navigation
    public Campaigns.Entities.Campaign Campaign { get; set; } = null!;
    public ICollection<Content.Entities.ContentItem> ContentItems { get; set; } = new List<Content.Entities.ContentItem>();
}
