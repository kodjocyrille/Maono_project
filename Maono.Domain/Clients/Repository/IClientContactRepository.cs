using Maono.Domain.Clients.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Clients.Repository;

public interface IClientContactRepository : IBaseRepository<ClientContact>
{
    Task<IReadOnlyList<ClientContact>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<ClientContact?> GetPrimaryApproverAsync(Guid organizationId, CancellationToken ct = default);
}
