using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Missions.DTOs;

namespace Maono.Application.Features.Missions.Queries;

public record GetMissionByIdQuery(Guid Id) : IQuery<Result<MissionDetailDto>>;
public record ListMissionsQuery(string? Status = null) : IQuery<Result<List<MissionDto>>>;
