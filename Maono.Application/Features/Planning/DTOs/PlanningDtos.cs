namespace Maono.Application.Features.Planning.DTOs;

public record CalendarEntryDto(Guid Id, Guid CampaignId, DateTime PublicationDate, string? Platform, string? ContentType, string? Theme, string? Status, DateTime CreatedAtUtc);
public record ResourceCapacityDto(Guid Id, Guid UserId, DateTime WeekStart, decimal CapacityHours, decimal AssignedHours, decimal? OverloadThreshold);
