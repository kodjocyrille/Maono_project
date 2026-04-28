using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IMembershipRepository : IBaseRepository<WorkspaceMembership>
{
    Task<IReadOnlyList<WorkspaceMembership>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default);
    Task<WorkspaceMembership?> GetByUserAndWorkspaceAsync(Guid userId, Guid workspaceId, CancellationToken ct = default);
    Task<IReadOnlyList<WorkspaceMembership>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Workspace?> GetFirstWorkspaceAsync(CancellationToken ct = default);
    Task<bool> IsLastAdminAsync(Guid workspaceId, Guid membershipId, CancellationToken ct = default);
    Task<WorkspaceMembership?> GetByIdWithNavigationsAsync(Guid id, CancellationToken ct = default);
}
