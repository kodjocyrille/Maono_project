using Maono.Domain.Common;
using Maono.Domain.Content.Entities;

namespace Maono.Domain.Content.Repository;

public interface IContentTaskRepository : IBaseRepository<ContentTask>
{
    Task<IReadOnlyList<ContentTask>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default);
    Task<IReadOnlyList<ContentTask>> GetByAssignedUserAsync(Guid userId, ContentTaskStatus? status, CancellationToken ct = default);
}
