using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithMembershipsAsync(Guid id, CancellationToken ct = default);
    Task<List<User>> GetAllWithMembershipsAsync(CancellationToken ct = default);
}
