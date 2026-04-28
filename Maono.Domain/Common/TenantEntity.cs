namespace Maono.Domain.Common;

/// <summary>
/// Base for all tenant-owned entities. Adds WorkspaceId for multi-tenant isolation.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid WorkspaceId { get; set; }
}
