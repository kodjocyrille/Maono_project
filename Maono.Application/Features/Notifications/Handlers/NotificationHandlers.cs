using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Notifications.Commands;
using Maono.Application.Features.Notifications.DTOs;
using Maono.Application.Features.Notifications.Queries;
using Maono.Domain.Notifications.Repository;
using MediatR;

namespace Maono.Application.Features.Notifications.Handlers;

public class ListNotificationsHandler : IRequestHandler<ListNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly INotificationRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public ListNotificationsHandler(INotificationRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<List<NotificationDto>>> Handle(ListNotificationsQuery request, CancellationToken ct)
    {
        var notifs = await _repo.GetUnreadByUserAsync(_currentUser.UserId!.Value, ct);
        var dtos = notifs.Select(n => new NotificationDto(n.Id, n.Type, n.Subject, n.Body, n.Status, n.SentAtUtc, n.ReadAtUtc, n.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly INotificationRepository _repo;
    public MarkNotificationReadHandler(INotificationRepository repo) => _repo = repo;

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notif = await _repo.GetByIdAsync(request.Id, ct);
        if (notif == null) return Result.Failure("Notification not found", "NOT_FOUND");
        notif.ReadAtUtc = DateTime.UtcNow;
        _repo.Update(notif);
        return Result.Success();
    }
}

public class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    private readonly INotificationRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsReadHandler(INotificationRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        await _repo.MarkAllAsReadAsync(_currentUser.UserId!.Value, ct);
        return Result.Success();
    }
}
