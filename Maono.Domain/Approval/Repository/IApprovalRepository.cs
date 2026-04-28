using Maono.Domain.Approval.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Approval.Repository;

public interface IApprovalRepository : IBaseRepository<ApprovalCycle>
{
    Task<ApprovalCycle?> GetWithDecisionsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalCycle>> GetByContentAsync(Guid contentItemId, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalCycle>> GetPendingAsync(CancellationToken ct = default);
}
