using Maono.Domain.Publications.Entities;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Publications.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class PublicationRepository : BaseRepository<Publication>, IPublicationRepository
{
    public PublicationRepository(MaonoDbContext context) : base(context) { }

    public async Task<Publication?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Variants)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Publication>> GetScheduledBeforeAsync(DateTime utcTime, CancellationToken ct = default)
        => await DbSet.Where(p => p.ScheduledAtUtc <= utcTime && p.Status == PublicationStatus.Scheduled).ToListAsync(ct);

    public async Task<IReadOnlyList<Publication>> GetByStatusAsync(PublicationStatus status, CancellationToken ct = default)
        => await DbSet.Where(p => p.Status == status).ToListAsync(ct);
}
