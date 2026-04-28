namespace Maono.Domain.Common;

/// <summary>
/// Base class for aggregate roots. Combines entity identity, audit and domain events
/// with the aggregate root marker.
/// </summary>
public abstract class AggregateRoot : BaseEntity, IAggregateRoot
{
}
