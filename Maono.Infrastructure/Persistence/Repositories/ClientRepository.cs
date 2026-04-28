using Maono.Domain.Clients.Entities;
using Maono.Domain.Clients.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class ClientRepository : BaseRepository<ClientOrganization>, IClientRepository
{
    public ClientRepository(MaonoDbContext context) : base(context) { }

    public async Task<ClientOrganization?> GetWithContactsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.Contacts).Include(c => c.BrandProfile).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<ClientOrganization?> GetWithBrandProfileAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.BrandProfile).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ClientOrganization>> SearchByNameAsync(string name, CancellationToken ct = default)
        => await DbSet.Where(c => c.Name.Contains(name)).ToListAsync(ct);
}
