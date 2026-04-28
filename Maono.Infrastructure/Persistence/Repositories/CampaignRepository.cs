using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Campaigns.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class CampaignRepository : BaseRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(MaonoDbContext context) : base(context) { }

    public async Task<Campaign?> GetWithKpisAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.Kpis).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken ct = default)
        => await DbSet.Where(c => c.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<Campaign>> GetByClientAsync(Guid clientOrganizationId, CancellationToken ct = default)
        => await DbSet.Where(c => c.ClientOrganizationId == clientOrganizationId).ToListAsync(ct);
}
