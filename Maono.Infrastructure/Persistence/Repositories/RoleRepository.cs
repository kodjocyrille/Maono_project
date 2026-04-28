using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    public RoleRepository(MaonoDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<Role>> GetAllSystemRolesAsync(CancellationToken ct = default)
        => await DbSet
            .Where(r => r.IsSystem)
            .Include(r => r.Permissions)
            .ToListAsync(ct);

    public async Task<List<Role>> GetAllWithPermissionsAsync(CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
}
