using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.DTOs;

namespace Maono.Application.Features.Assets.Queries;

/// <summary>
/// ECR-020 — Search assets by type, content, or filename.
/// </summary>
public record SearchAssetsQuery(
    string? FileName,
    string? AssetType,
    Guid? ContentItemId
) : IQuery<Result<List<AssetSummaryDto>>>;

public record AssetSummaryDto(
    Guid Id,
    Guid ContentItemId,
    string? AssetType,
    string? OriginalFileName,
    string? MimeType,
    int CurrentVersionNumber,
    DateTime CreatedAtUtc
);
