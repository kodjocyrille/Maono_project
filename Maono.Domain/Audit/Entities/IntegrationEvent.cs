using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Audit.Entities;

/// <summary>
/// Lightweight outbox for integration events.
/// </summary>
public class IntegrationEvent : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? AggregateId { get; set; }
    public string? PayloadJson { get; set; }
    public string? Status { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}
