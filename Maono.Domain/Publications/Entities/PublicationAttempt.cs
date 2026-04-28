using Maono.Domain.Publications.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Publications.Entities;

public class PublicationAttempt : TenantEntity
{
    public Guid PublicationId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation
    public Publication Publication { get; set; } = null!;
}
