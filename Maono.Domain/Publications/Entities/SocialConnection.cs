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

public class SocialConnection : TenantEntity
{
    public SocialPlatform Platform { get; set; }
    public string ExternalAccountId { get; set; } = string.Empty;
    public string? AccessTokenRef { get; set; }
    public string? RefreshTokenRef { get; set; }
    public string? AccountName { get; set; }
    public string? Status { get; set; }
    public DateTime? ConnectedAtUtc { get; set; }
    public DateTime? TokenExpiresAtUtc { get; set; }
}
