using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Performance.DTOs;

namespace Maono.Application.Features.Performance.Queries;

public record GetPublicationPerformanceQuery(Guid PublicationId) : IQuery<Result<List<PerformanceSnapshotDto>>>;
public record GetCampaignPerformanceQuery(Guid CampaignId) : IQuery<Result<List<CampaignPerformanceSummaryDto>>>;
public record GetCampaignPerformanceSummaryQuery(Guid CampaignId) : IQuery<Result<CampaignPerformanceSummaryDto>>;
