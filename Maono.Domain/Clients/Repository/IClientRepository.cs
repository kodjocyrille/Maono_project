using Maono.Domain.Clients.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Clients.Repository;

public interface IClientRepository : IBaseRepository<ClientOrganization>
{
    Task<ClientOrganization?> GetWithContactsAsync(Guid id, CancellationToken ct = default);
    Task<ClientOrganization?> GetWithBrandProfileAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ClientOrganization>> SearchByNameAsync(string name, CancellationToken ct = default);
}
