using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Audit.Entities;

public class IntegrationFailure : BaseEntity
{
    public string Provider { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? ErrorCode { get; set; }
    public string? Details { get; set; }
    public DateTime FailedAtUtc { get; set; } = DateTime.UtcNow;
}
