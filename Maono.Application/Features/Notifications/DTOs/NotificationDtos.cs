namespace Maono.Application.Features.Notifications.DTOs;

public record NotificationDto(Guid Id, string Type, string Subject, string? Body, string? Status, DateTime? SentAtUtc, DateTime? ReadAtUtc, DateTime CreatedAtUtc);
