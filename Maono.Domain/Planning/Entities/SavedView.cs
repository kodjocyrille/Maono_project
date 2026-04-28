using Maono.Domain.Planning.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Planning.Entities;

public class SavedView : TenantEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FiltersJson { get; set; }
}
