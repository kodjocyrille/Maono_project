using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Identity.Repository;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class MembershipRepository : BaseRepository<WorkspaceMembership>, IMembershipRepository
{
    public MembershipRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<WorkspaceMembership>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct = default)
        => await Context.Set<WorkspaceMembership>()
            .Include(m => m.User)
            .Include(m => m.Role)
            .Where(m => m.WorkspaceId == workspaceId && m.Status == MembershipStatus.Active)
            .ToListAsync(ct);

    public async Task<WorkspaceMembership?> GetByUserAndWorkspaceAsync(Guid userId, Guid workspaceId, CancellationToken ct = default)
        => await Context.Set<WorkspaceMembership>()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.WorkspaceId == workspaceId, ct);

    public async Task<IReadOnlyList<WorkspaceMembership>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Context.Set<WorkspaceMembership>()
            .Include(m => m.Role)
            .Include(m => m.Workspace)
            .Where(m => m.UserId == userId && m.Status == MembershipStatus.Active)
            .ToListAsync(ct);

    public async Task<Workspace?> GetFirstWorkspaceAsync(CancellationToken ct = default)
        => await Context.Workspaces.FirstOrDefaultAsync(ct);

    public async Task<bool> IsLastAdminAsync(Guid workspaceId, Guid membershipId, CancellationToken ct = default)
    {
        var adminCount = await Context.Set<WorkspaceMembership>()
            .Include(m => m.Role)
            .CountAsync(m => m.WorkspaceId == workspaceId
                && m.Status == MembershipStatus.Active
                && m.Role != null && m.Role.Name == "Admin", ct);

        if (adminCount > 1) return false;

        // Check if the one being removed IS an admin
        var membership = await Context.Set<WorkspaceMembership>()
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == membershipId, ct);

        return membership?.Role?.Name == "Admin";
    }

    public async Task<WorkspaceMembership?> GetByIdWithNavigationsAsync(Guid id, CancellationToken ct = default)
        => await Context.Set<WorkspaceMembership>()
            .Include(m => m.User)
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
}
