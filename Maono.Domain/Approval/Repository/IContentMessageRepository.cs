using Maono.Domain.Approval.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Approval.Repository;

public interface IContentMessageRepository : IBaseRepository<ContentMessage>
{
    Task<IReadOnlyList<ContentMessage>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default);
}
