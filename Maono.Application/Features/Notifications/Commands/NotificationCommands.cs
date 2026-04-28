using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Notifications.Commands;

public record MarkNotificationReadCommand(Guid Id) : ICommand<Result>;
public record MarkAllNotificationsReadCommand() : ICommand<Result>;
