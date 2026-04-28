using Maono.Domain.Common;

namespace Maono.Domain.Campaigns.Entities;

/// <summary>
/// ECR-014 — Individual expense line within a campaign budget.
/// Tracks real spending against BudgetPlanned.
/// </summary>
public class CampaignExpense : TenantEntity
{
    public Guid CampaignId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; }

    // Navigation
    public Campaign Campaign { get; set; } = null!;
}
