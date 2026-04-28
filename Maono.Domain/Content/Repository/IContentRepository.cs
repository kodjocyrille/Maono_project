using Maono.Domain.Common;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Enums;

namespace Maono.Domain.Content.Repository;

public interface IContentRepository : IBaseRepository<ContentItem>
{
    Task<ContentItem?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ContentItem>> GetByStatusAsync(ContentStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<ContentItem>> GetApproachingDeadlineAsync(DateTime deadline, CancellationToken ct = default);
}
