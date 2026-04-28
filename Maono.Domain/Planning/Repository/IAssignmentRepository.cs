using Maono.Domain.Common;
using Maono.Domain.Planning.Entities;

namespace Maono.Domain.Planning.Repository;

public interface IAssignmentRepository : IBaseRepository<Assignment>
{
    Task<IReadOnlyList<Assignment>> GetByUserAsync(Guid userId, CancellationToken ct = default);
}
