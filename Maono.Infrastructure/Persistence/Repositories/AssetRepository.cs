using Maono.Domain.Assets.Entities;
using Maono.Domain.Assets.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class AssetRepository : BaseRepository<Asset>, IAssetRepository
{
    public AssetRepository(MaonoDbContext context) : base(context) { }

    public async Task<Asset?> GetWithVersionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(a => a.Versions).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Asset>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default)
        => await DbSet.Where(a => a.ContentItemId == contentItemId).ToListAsync(ct);
}
