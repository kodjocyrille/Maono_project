using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Portal.Commands;

public record GeneratePortalTokenCommand(
    Guid ClientOrganizationId,
    Guid? ContentItemId,
    Guid? CampaignId,
    int ExpiryHours = 72
) : ICommand<Result<PortalTokenDto>>;

public record RevokePortalTokenCommand(
    Guid TokenId,
    string? Reason
) : ICommand<Result>;

public record SubmitPortalDecisionCommand(
    string Token,
    Guid ContentItemId,
    string Decision,     // "approved" | "changes_requested"
    string? Comment
) : ICommand<Result<PortalDecisionDto>>;

public record PortalTokenDto(
    Guid TokenId,
    string Token,
    string PortalUrl,    // full URL with token for sharing with client
    DateTime ExpiresAt
);

public record PortalDecisionDto(
    Guid ContentItemId,
    string Decision,
    string? Comment,
    DateTime SubmittedAt
);
