using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Common;
using Maono.Domain.Notifications.Entities;
using Maono.Domain.Notifications.Enums;
using MediatR;

namespace Maono.Application.Features.Notifications;

// ── ECR-024/025 — Notification Preferences CRUD ────────────

public record GetNotificationPreferencesQuery() : IQuery<Result<List<NotificationPreferenceDto>>>;
public record UpdateNotificationPreferenceCommand(
    NotificationChannel Channel,
    bool Enabled,
    string? DigestMode
) : ICommand<Result<NotificationPreferenceDto>>;

public record NotificationPreferenceDto(
    Guid Id,
    Guid UserId,
    string Channel,
    bool Enabled,
    string? DigestMode
);

public class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, Result<List<NotificationPreferenceDto>>>
{
    private readonly IGenericRepository<NotificationPreference> _prefRepo;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationPreferencesHandler(IGenericRepository<NotificationPreference> prefRepo, ICurrentUserService currentUser)
    {
        _prefRepo = prefRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationPreferenceDto>>> Handle(GetNotificationPreferencesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var prefs = await _prefRepo.FindAsync(p => p.UserId == userId, ct);

        // If no preferences exist, return defaults
        if (prefs.Count == 0)
        {
            return Result.Success(new List<NotificationPreferenceDto>
            {
                new(Guid.Empty, userId, NotificationChannel.InApp.ToString(), true, null),
                new(Guid.Empty, userId, NotificationChannel.Email.ToString(), true, null),
                new(Guid.Empty, userId, NotificationChannel.Sms.ToString(), false, null)
            });
        }

        return Result.Success(prefs.Select(p => new NotificationPreferenceDto(
            p.Id, p.UserId, p.Channel.ToString(), p.Enabled, p.DigestMode)).ToList());
    }
}

public class UpdateNotificationPreferenceHandler : IRequestHandler<UpdateNotificationPreferenceCommand, Result<NotificationPreferenceDto>>
{
    private readonly IGenericRepository<NotificationPreference> _prefRepo;
    private readonly ICurrentUserService _currentUser;

    public UpdateNotificationPreferenceHandler(IGenericRepository<NotificationPreference> prefRepo, ICurrentUserService currentUser)
    {
        _prefRepo = prefRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<NotificationPreferenceDto>> Handle(UpdateNotificationPreferenceCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var existing = await _prefRepo.FirstOrDefaultAsync(
            p => p.UserId == userId && p.Channel == request.Channel, ct);

        if (existing != null)
        {
            existing.Enabled = request.Enabled;
            existing.DigestMode = request.DigestMode;
        }
        else
        {
            existing = new NotificationPreference
            {
                UserId = userId,
                Channel = request.Channel,
                Enabled = request.Enabled,
                DigestMode = request.DigestMode
            };
            await _prefRepo.AddAsync(existing, ct);
        }

        return Result.Success(new NotificationPreferenceDto(
            existing.Id, userId, request.Channel.ToString(), request.Enabled, request.DigestMode));
    }
}
