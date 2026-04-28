using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MaonoDbContext context) : base(context) { }

    public async Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetWithMembershipsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(u => u.Memberships).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<List<User>> GetAllWithMembershipsAsync(CancellationToken ct = default)
        => await DbSet
            .Include(u => u.Memberships)
                .ThenInclude(m => m.Workspace)
            .Include(u => u.Memberships)
                .ThenInclude(m => m.Role)
                    .ThenInclude(r => r.Permissions)
            .ToListAsync(ct);
}
