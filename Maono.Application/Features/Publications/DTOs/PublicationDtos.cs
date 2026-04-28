using Maono.Domain.Publications.Enums;

namespace Maono.Application.Features.Publications.DTOs;

public record PublicationDto(Guid Id, Guid ContentItemId, SocialPlatform Platform, PublicationStatus Status, DateTime? ScheduledAtUtc, DateTime CreatedAtUtc);
public record PublicationDetailDto(Guid Id, Guid ContentItemId, SocialPlatform Platform, PublicationStatus Status, DateTime? ScheduledAtUtc, DateTime? PublishedAtUtc, string? ExternalPostId, DateTime CreatedAtUtc, List<PublicationAttemptDto> Attempts);
public record PublicationAttemptDto(Guid Id, int AttemptNumber, DateTime StartedAtUtc, string? Result, string? ErrorMessage);
