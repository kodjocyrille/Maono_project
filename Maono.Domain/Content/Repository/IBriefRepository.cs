using Maono.Domain.Common;
using Maono.Domain.Content.Entities;

namespace Maono.Domain.Content.Repository;

public interface IBriefRepository : IBaseRepository<Brief>
{
    Task<IReadOnlyList<Brief>> GetByContentAsync(Guid contentItemId, CancellationToken ct = default);
}
