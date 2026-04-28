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

namespace Maono.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceName,
    DeviceType DeviceType,
    string? UserAgent,
    string? IpAddress,
    string? DeviceFingerprint
) : ICommand<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Email,
    string DisplayName,
    Guid? WorkspaceId,
    Guid SessionId
);
