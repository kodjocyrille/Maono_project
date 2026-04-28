using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Messages.Queries;

/// <summary>
/// ECR-008a — Export all messages for a campaign (across all content items).
/// </summary>
public record ExportCampaignMessagesQuery(Guid CampaignId) : IQuery<Result<List<CampaignMessageExportDto>>>;

public record CampaignMessageExportDto(
    Guid ContentItemId,
    string ContentTitle,
    Guid MessageId,
    string AuthorType,
    string Body,
    DateTime SentAtUtc
);
