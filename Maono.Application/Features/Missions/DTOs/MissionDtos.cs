using Maono.Domain.Missions.Enums;

namespace Maono.Application.Features.Missions.DTOs;

public record MissionDto(
    Guid Id,
    string Name,
    MissionStatus Status,
    Guid OwnerUserId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    DateTime CreatedAtUtc
);

public record MissionDetailDto(
    Guid Id,
    string Name,
    string? Description,
    MissionStatus Status,
    Guid OwnerUserId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    DateTime CreatedAtUtc,
    List<MissionMemberDto> Members,
    List<MissionMilestoneDto> Milestones
);

public record MissionMemberDto(Guid Id, Guid UserId, string? RoleOnMission);
public record MissionMilestoneDto(Guid Id, string Name, DateTime? DueDate, string? Status);
