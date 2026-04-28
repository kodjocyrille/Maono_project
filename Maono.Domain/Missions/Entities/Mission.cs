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

namespace Maono.Domain.Missions.Entities;

/// <summary>
/// Mission aggregate root for freelance mode.
/// </summary>
public class Mission : TenantAggregateRoot, ISoftDeletable
{
    public Guid OwnerUserId { get; set; }
    public Guid? ClientOrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MissionStatus Status { get; set; } = MissionStatus.Brief;
    public decimal? Budget { get; set; }
    public string? Currency { get; set; } = "EUR";
    public BillingStatus InvoiceStatus { get; set; } = BillingStatus.Draft;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<MissionMember> Members { get; set; } = new List<MissionMember>();
    public ICollection<MissionMilestone> Milestones { get; set; } = new List<MissionMilestone>();
    public ICollection<MissionTask> Tasks { get; set; } = new List<MissionTask>();
    public ICollection<BillingRecord> BillingRecords { get; set; } = new List<BillingRecord>();
}
