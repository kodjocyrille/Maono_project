using Maono.Domain.Missions.Entities;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Missions.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class MissionRepository : BaseRepository<Mission>, IMissionRepository
{
    public MissionRepository(MaonoDbContext context) : base(context) { }

    public async Task<Mission?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(m => m.Members)
            .Include(m => m.Milestones)
            .Include(m => m.BillingRecords)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Mission>> GetByStatusAsync(MissionStatus status, CancellationToken ct = default)
        => await DbSet.Where(m => m.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<Mission>> GetByOwnerAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.Where(m => m.OwnerUserId == userId).ToListAsync(ct);
}
