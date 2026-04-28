using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;

namespace Maono.Application.Features.Auth.Queries;

/// <summary>
/// Returns all active sessions for the current user.
/// </summary>
public record GetActiveSessionsQuery(Guid UserId) : IQuery<Result<List<SessionDto>>>;

public record SessionDto(
    Guid SessionId,
    DeviceType DeviceType,
    string? DeviceName,
    string? IpAddress,
    DateTime LoginAtUtc,
    DateTime LastActiveAtUtc,
    bool IsCurrent
);
