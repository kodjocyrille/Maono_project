using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Notifications.DTOs;

namespace Maono.Application.Features.Notifications.Queries;

public record ListNotificationsQuery() : IQuery<Result<List<NotificationDto>>>;
