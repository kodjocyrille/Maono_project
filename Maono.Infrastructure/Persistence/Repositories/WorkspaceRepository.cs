using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class WorkspaceRepository : BaseRepository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(MaonoDbContext context) : base(context) { }

    public async Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(w => w.Slug == slug, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => await DbSet.AnyAsync(w => w.Slug == slug, ct);
}
