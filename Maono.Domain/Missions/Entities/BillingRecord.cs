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

public class BillingRecord : TenantEntity
{
    public Guid MissionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Draft;
    public DateTime? ExportedToOdooAtUtc { get; set; }
    public string? OdooInvoiceId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Mission Mission { get; set; } = null!;
}
