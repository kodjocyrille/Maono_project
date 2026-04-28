using Maono.Domain.Assets.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Assets.Repository;

public interface IAssetRepository : IBaseRepository<Asset>
{
    Task<Asset?> GetWithVersionsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Asset>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default);
}
