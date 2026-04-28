using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default);
    Task<List<Role>> GetAllSystemRolesAsync(CancellationToken ct = default);
    Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken ct = default);
}
