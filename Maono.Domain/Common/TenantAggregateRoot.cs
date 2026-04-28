namespace Maono.Domain.Common;

/// <summary>
/// Tenant-scoped aggregate root. For aggregates that belong to a specific workspace.
/// </summary>
public abstract class TenantAggregateRoot : TenantEntity, IAggregateRoot
{
}
