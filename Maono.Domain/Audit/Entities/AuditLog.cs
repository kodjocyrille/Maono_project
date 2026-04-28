using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Audit.Entities;

public class AuditLog : TenantEntity
{
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? ActorType { get; set; }
    public string? ActorId { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
