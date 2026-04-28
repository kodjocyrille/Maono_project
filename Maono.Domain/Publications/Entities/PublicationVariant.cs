using Maono.Domain.Publications.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Publications.Entities;

public class PublicationVariant : TenantEntity
{
    public Guid PublicationId { get; set; }
    public string? Caption { get; set; }
    public string? HashTags { get; set; }
    public string? PlatformSpecificPayload { get; set; }
    public string? Ratio { get; set; }

    // Navigation
    public Publication Publication { get; set; } = null!;
}
