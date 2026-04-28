using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IWorkspaceRepository : IBaseRepository<Workspace>
{
    Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}
