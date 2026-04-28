using Maono.Domain.Assets.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Assets.Repository;

public interface IAssetVersionRepository : IBaseRepository<AssetVersion>
{
    Task<AssetVersion?> GetByVersionNumberAsync(Guid assetId, int versionNumber, CancellationToken ct = default);
    Task<IReadOnlyList<AssetVersion>> GetByAssetAsync(Guid assetId, CancellationToken ct = default);
}
