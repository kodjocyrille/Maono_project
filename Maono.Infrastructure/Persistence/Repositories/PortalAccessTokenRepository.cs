using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Repository;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class PortalAccessTokenRepository : BaseRepository<PortalAccessToken>, IPortalAccessTokenRepository
{
    public PortalAccessTokenRepository(MaonoDbContext context) : base(context) { }

    public async Task<PortalAccessToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await Context.Set<PortalAccessToken>()
            .FirstOrDefaultAsync(t => t.Token == token, ct);

    public async Task<IReadOnlyList<PortalAccessToken>> GetByClientAsync(Guid clientOrganizationId, CancellationToken ct = default)
        => await Context.Set<PortalAccessToken>()
            .Where(t => t.ClientOrganizationId == clientOrganizationId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
}
