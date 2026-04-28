using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.DTOs;

namespace Maono.Application.Features.Assets.Queries;

public record GetAssetByIdQuery(Guid Id) : IQuery<Result<AssetDetailDto>>;
public record GetAssetVersionsQuery(Guid AssetId) : IQuery<Result<List<AssetVersionDto>>>;
