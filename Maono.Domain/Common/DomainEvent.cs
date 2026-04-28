namespace Maono.Domain.Common;

/// <summary>
/// Base class for domain events. Domain events are raised by entities
/// and dispatched after persistence to notify other parts of the system.
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
