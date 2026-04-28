using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IPermissionRepository : IBaseRepository<Permission>
{
    Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
}
