using Maono.Domain.Notifications.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Notifications.Entities;

public class EscalationRule : TenantEntity
{
    public string TriggerType { get; set; } = string.Empty;
    public TimeSpan Delay { get; set; }
    public string? RecipientsRule { get; set; }
    public bool IsEnabled { get; set; } = true;
}
