namespace Maono.Domain.Content.Enums;

/// <summary>
/// Lifecycle states of a content item.
/// Transitions are enforced by the application layer state machine (ECR-001).
/// Valid flow: Draft → InProduction → InReview → ClientReview → Approved → Scheduled → Published → Archived
/// Special: any reviewable state → RevisionRequired → InProduction (on client rejection)
/// </summary>
public enum ContentStatus
{
    Draft = 0,
    InProduction = 1,
    InReview = 2,          // Internal review
    ClientReview = 3,      // Client review (requires InternalApproval first)
    RevisionRequired = 4,  // Client rejected — back to production
    Approved = 5,
    Scheduled = 6,
    Published = 7,
    PublishFailed = 8,
    Archived = 9
}
