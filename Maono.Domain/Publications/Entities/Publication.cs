using Maono.Domain.Notifications.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Content.Entities;
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

namespace Maono.Domain.Publications.Entities;

/// <summary>
/// Publication aggregate root. Handles scheduling, publishing and retry.
/// </summary>
public class Publication : TenantAggregateRoot
{
    public Guid ContentItemId { get; set; }
    public SocialPlatform Platform { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;
    public string? ExternalPostId { get; set; }

    // Navigation
    public Content.Entities.ContentItem ContentItem { get; set; } = null!;
    public ICollection<PublicationVariant> Variants { get; set; } = new List<PublicationVariant>();
    public ICollection<PublicationAttempt> Attempts { get; set; } = new List<PublicationAttempt>();
}
