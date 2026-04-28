using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Audit.Entities;

public class WebhookSubscription : TenantEntity
{
    public string Endpoint { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public string? EventFilter { get; set; }
    public bool IsEnabled { get; set; } = true;
}
