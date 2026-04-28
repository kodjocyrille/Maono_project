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

namespace Maono.Domain.Approval.Entities;

/// <summary>
/// Approval cycle for a content item. Tracks internal and client approval status.
/// </summary>
public class ApprovalCycle : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public int RevisionRound { get; set; } = 1;
    public ApprovalStatus InternalStatus { get; set; } = ApprovalStatus.Pending;
    public ApprovalStatus ClientStatus { get; set; } = ApprovalStatus.Pending;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>ECR-016 — Configurable deadline for this approval cycle.</summary>
    public DateTime? DeadlineUtc { get; set; }
    /// <summary>ECR-016 — Number of reminders already sent (for escalation control).</summary>
    public int ReminderSentCount { get; set; }

    // Navigation
    public Content.Entities.ContentItem ContentItem { get; set; } = null!;
    public ICollection<ApprovalDecision> Decisions { get; set; } = new List<ApprovalDecision>();
}
