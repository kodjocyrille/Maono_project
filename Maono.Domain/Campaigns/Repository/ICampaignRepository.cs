using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Common;

namespace Maono.Domain.Campaigns.Repository;

public interface ICampaignRepository : IBaseRepository<Campaign>
{
    Task<Campaign?> GetWithKpisAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Campaign>> GetByClientAsync(Guid clientOrganizationId, CancellationToken ct = default);
}
