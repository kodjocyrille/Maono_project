using Maono.Domain.Common;

namespace Maono.Domain.Content.Entities;

/// <summary>
/// A production task linked to a content item.
/// Tracks granular work items assigned to team members.
/// </summary>
public class ContentTask : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ContentTaskStatus Status { get; set; } = ContentTaskStatus.Pending;
    public ContentTaskPriority Priority { get; set; } = ContentTaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public string? BlockedReason { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    // Navigation
    public ContentItem ContentItem { get; set; } = null!;
}

public enum ContentTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Blocked = 3
}

public enum ContentTaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
