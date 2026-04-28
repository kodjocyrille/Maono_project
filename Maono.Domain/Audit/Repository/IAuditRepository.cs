using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Audit.Repository;

public interface IAuditRepository : IBaseRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}
