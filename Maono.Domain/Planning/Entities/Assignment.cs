using Maono.Domain.Content.Entities;
using Maono.Domain.Planning.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Planning.Entities;

public class Assignment : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public Guid UserId { get; set; }
    public string? RoleOnTask { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Content.Entities.ContentItem ContentItem { get; set; } = null!;
}
