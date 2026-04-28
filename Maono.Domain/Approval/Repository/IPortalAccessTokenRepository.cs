using Maono.Domain.Common;
using Maono.Domain.Approval.Entities;

namespace Maono.Domain.Approval.Repository;

public interface IPortalAccessTokenRepository : IBaseRepository<PortalAccessToken>
{
    Task<PortalAccessToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<PortalAccessToken>> GetByClientAsync(Guid clientOrganizationId, CancellationToken ct = default);
}
