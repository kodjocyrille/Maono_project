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

namespace Maono.Domain.Campaigns.Entities;

/// <summary>
/// Campaign aggregate root. Groups objectives, KPIs, calendar and contents.
/// </summary>
public class Campaign : TenantAggregateRoot, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public Guid ClientOrganizationId { get; set; }
    public string? Objective { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public decimal? BudgetPlanned { get; set; }
    public decimal? BudgetSpent { get; set; }
    public string? Description { get; set; }

    // KPI Targets (P7)
    public long? TargetReach { get; set; }
    public decimal? TargetCtr { get; set; }
    public long? TargetConversions { get; set; }
    public decimal? TargetEngagementRate { get; set; }
    public string[]? TargetPlatforms { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<CampaignKpi> Kpis { get; set; } = new List<CampaignKpi>();
    public ICollection<CampaignTag> Tags { get; set; } = new List<CampaignTag>();
    public CampaignClosureRecord? ClosureRecord { get; set; }
}
