using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Missions.DTOs;

namespace Maono.Application.Features.Missions.Commands;

public record CreateMissionCommand(
    string Name,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget
) : ICommand<Result<MissionDto>>;

public record UpdateMissionStatusCommand(
    Guid Id,
    Domain.Missions.Enums.MissionStatus NewStatus
) : ICommand<Result<MissionDto>>;
