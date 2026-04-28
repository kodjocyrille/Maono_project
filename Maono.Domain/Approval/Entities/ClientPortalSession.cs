using Maono.Domain.Approval.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Approval.Entities;

public class ClientPortalSession : TenantEntity
{
    public Guid? ContactId { get; set; }
    public Guid ApprovalCycleId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }

    // Navigation
    public ApprovalCycle ApprovalCycle { get; set; } = null!;
}
