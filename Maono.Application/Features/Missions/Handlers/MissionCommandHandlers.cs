using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Missions.Commands;
using Maono.Application.Features.Missions.DTOs;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Missions.Repository;
using MediatR;

namespace Maono.Application.Features.Missions.Handlers;

public class CreateMissionHandler : IRequestHandler<CreateMissionCommand, Result<MissionDto>>
{
    private readonly IMissionRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public CreateMissionHandler(IMissionRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<MissionDto>> Handle(CreateMissionCommand request, CancellationToken ct)
    {
        var mission = new Mission
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Name = request.Name,
            Description = request.Description,
            OwnerUserId = _currentUser.UserId!.Value,
            Status = MissionStatus.Brief,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Budget = request.Budget
        };
        await _repo.AddAsync(mission, ct);

        return Result.Success(new MissionDto(
            mission.Id, mission.Name, mission.Status, mission.OwnerUserId,
            mission.StartDate, mission.EndDate, mission.Budget, mission.CreatedAtUtc));
    }
}

public class UpdateMissionStatusHandler : IRequestHandler<UpdateMissionStatusCommand, Result<MissionDto>>
{
    private readonly IMissionRepository _repo;

    public UpdateMissionStatusHandler(IMissionRepository repo) => _repo = repo;

    public async Task<Result<MissionDto>> Handle(UpdateMissionStatusCommand request, CancellationToken ct)
    {
        var mission = await _repo.GetByIdAsync(request.Id, ct);
        if (mission == null) return Result.Failure<MissionDto>("Mission not found", "NOT_FOUND");

        mission.Status = request.NewStatus;
        _repo.Update(mission);

        return Result.Success(new MissionDto(
            mission.Id, mission.Name, mission.Status, mission.OwnerUserId,
            mission.StartDate, mission.EndDate, mission.Budget, mission.CreatedAtUtc));
    }
}

public class UpdateMissionHandler : IRequestHandler<UpdateMissionCommand, Result>
{
    private readonly IMissionRepository _repo;
    public UpdateMissionHandler(IMissionRepository repo) => _repo = repo;

    public async Task<Result> Handle(UpdateMissionCommand request, CancellationToken ct)
    {
        var mission = await _repo.GetByIdAsync(request.Id, ct);
        if (mission == null) return Result.Failure("Mission not found", "NOT_FOUND");
        mission.Name = request.Name;
        if (request.Description != null) mission.Description = request.Description;
        if (request.Budget.HasValue) mission.Budget = request.Budget;
        if (request.StartDate.HasValue) mission.StartDate = request.StartDate;
        if (request.EndDate.HasValue) mission.EndDate = request.EndDate;
        _repo.Update(mission);
        return Result.Success();
    }
}

public class DeleteMissionHandler : IRequestHandler<DeleteMissionCommand, Result>
{
    private readonly IMissionRepository _repo;
    public DeleteMissionHandler(IMissionRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteMissionCommand request, CancellationToken ct)
    {
        var mission = await _repo.GetByIdAsync(request.Id, ct);
        if (mission == null) return Result.Failure("Mission not found", "NOT_FOUND");
        mission.IsDeleted = true;
        mission.DeletedAtUtc = DateTime.UtcNow;
        _repo.Update(mission);
        return Result.Success();
    }
}

