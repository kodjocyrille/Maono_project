using Maono.Domain.Assets.Entities;
using Maono.Domain.Assets.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class AssetVersionRepository : BaseRepository<AssetVersion>, IAssetVersionRepository
{
    public AssetVersionRepository(MaonoDbContext context) : base(context) { }

    public async Task<AssetVersion?> GetByVersionNumberAsync(Guid assetId, int versionNumber, CancellationToken ct = default)
        => await Context.Set<AssetVersion>()
            .FirstOrDefaultAsync(v => v.AssetId == assetId && v.VersionNumber == versionNumber, ct);

    public async Task<IReadOnlyList<AssetVersion>> GetByAssetAsync(Guid assetId, CancellationToken ct = default)
        => await Context.Set<AssetVersion>()
            .Where(v => v.AssetId == assetId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(ct);
}
