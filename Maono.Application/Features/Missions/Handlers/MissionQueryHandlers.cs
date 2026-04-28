using Maono.Application.Common.Models;
using Maono.Application.Features.Missions.DTOs;
using Maono.Application.Features.Missions.Queries;
using Maono.Domain.Missions.Repository;
using MediatR;

namespace Maono.Application.Features.Missions.Handlers;

public class GetMissionByIdHandler : IRequestHandler<GetMissionByIdQuery, Result<MissionDetailDto>>
{
    private readonly IMissionRepository _repo;

    public GetMissionByIdHandler(IMissionRepository repo) => _repo = repo;

    public async Task<Result<MissionDetailDto>> Handle(GetMissionByIdQuery request, CancellationToken ct)
    {
        var mission = await _repo.GetWithDetailsAsync(request.Id, ct);
        if (mission == null) return Result.Failure<MissionDetailDto>("Mission not found", "NOT_FOUND");

        var members = mission.Members.Select(m => new MissionMemberDto(m.Id, m.UserId, m.RoleOnMission)).ToList();
        var milestones = mission.Milestones.Select(m => new MissionMilestoneDto(m.Id, m.Name, m.DueDate, m.Status)).ToList();

        return Result.Success(new MissionDetailDto(
            mission.Id, mission.Name, mission.Description, mission.Status,
            mission.OwnerUserId, mission.StartDate, mission.EndDate, mission.Budget,
            mission.CreatedAtUtc, members, milestones));
    }
}

public class ListMissionsHandler : IRequestHandler<ListMissionsQuery, Result<List<MissionDto>>>
{
    private readonly IMissionRepository _repo;

    public ListMissionsHandler(IMissionRepository repo) => _repo = repo;

    public async Task<Result<List<MissionDto>>> Handle(ListMissionsQuery request, CancellationToken ct)
    {
        var missions = await _repo.GetAllAsync(ct);
        var dtos = missions.Select(m => new MissionDto(
            m.Id, m.Name, m.Status, m.OwnerUserId,
            m.StartDate, m.EndDate, m.Budget, m.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}
