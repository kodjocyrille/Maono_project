using Maono.Domain.Common;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Publications.Enums;

namespace Maono.Domain.Publications.Repository;

public interface IPublicationRepository : IBaseRepository<Publication>
{
    Task<Publication?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Publication>> GetScheduledBeforeAsync(DateTime utcTime, CancellationToken ct = default);
    Task<IReadOnlyList<Publication>> GetByStatusAsync(PublicationStatus status, CancellationToken ct = default);
}
