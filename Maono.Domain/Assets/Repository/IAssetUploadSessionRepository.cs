using Maono.Domain.Common;
using Maono.Domain.Assets.Entities;

namespace Maono.Domain.Assets.Repository;

public interface IAssetUploadSessionRepository : IBaseRepository<AssetUploadSession>
{
    Task<AssetUploadSession?> GetPendingByIdAsync(Guid sessionId, CancellationToken ct = default);
}
