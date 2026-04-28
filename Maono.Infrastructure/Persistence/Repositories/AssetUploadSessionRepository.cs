using Maono.Domain.Assets.Entities;
using Maono.Domain.Assets.Repository;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class AssetUploadSessionRepository : BaseRepository<AssetUploadSession>, IAssetUploadSessionRepository
{
    public AssetUploadSessionRepository(MaonoDbContext context) : base(context) { }

    public async Task<AssetUploadSession?> GetPendingByIdAsync(Guid sessionId, CancellationToken ct = default)
        => await Context.Set<AssetUploadSession>()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == UploadSessionStatus.Pending, ct);
}
