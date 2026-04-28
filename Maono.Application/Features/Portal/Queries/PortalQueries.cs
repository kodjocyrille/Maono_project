using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Portal.Queries;

public record GetPortalContentsQuery(string Token) : IQuery<Result<PortalViewDto>>;

public record PortalViewDto(
    Guid ClientOrganizationId,
    string ClientName,
    List<PortalContentItemDto> Contents,
    DateTime ExpiresAt
);

public record PortalContentItemDto(
    Guid Id,
    string Title,
    string Status,
    string? Description,
    List<PortalAssetDto> Assets,
    string? PendingDecision
);

public record PortalAssetDto(
    Guid AssetId,
    string FileName,
    string MimeType,
    string SignedViewUrl    // 24h presigned GET URL
);
