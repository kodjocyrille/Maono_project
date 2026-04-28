using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Missions.Commands;

public record UpdateMissionCommand(Guid Id, string Name, string? Description, decimal? Budget, DateTime? StartDate, DateTime? EndDate) : ICommand<Result>;
public record DeleteMissionCommand(Guid Id) : ICommand<Result>;
