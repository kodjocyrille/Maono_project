using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(MaonoDbContext context) : base(context) { }

    public async Task<List<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
        => await DbSet
            .Where(p => codes.Contains(p.Code))
            .ToListAsync(ct);
}
