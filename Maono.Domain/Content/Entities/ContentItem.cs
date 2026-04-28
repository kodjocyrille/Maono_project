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

namespace Maono.Domain.Content.Entities;

/// <summary>
/// ContentItem aggregate root. Central entity for editorial production.
/// </summary>
public class ContentItem : TenantAggregateRoot, ISoftDeletable
{
    public Guid? CalendarEntryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Format { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public DateTime? Deadline { get; set; }
    public int Priority { get; set; }
    public int CurrentRevisionNumber { get; set; } = 1;

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Planning.Entities.CalendarEntry? CalendarEntry { get; set; }
    public ICollection<Brief> Briefs { get; set; } = new List<Brief>();
    public ICollection<TaskChecklistItem> ChecklistItems { get; set; } = new List<TaskChecklistItem>();
    public ICollection<Assets.Entities.Asset> Assets { get; set; } = new List<Assets.Entities.Asset>();
    public ICollection<Planning.Entities.Assignment> Assignments { get; set; } = new List<Planning.Entities.Assignment>();
}
