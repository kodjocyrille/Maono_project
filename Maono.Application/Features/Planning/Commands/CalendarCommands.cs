using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.DTOs;

namespace Maono.Application.Features.Planning.Commands;

public record CreateCalendarEntryCommand(
    Guid CampaignId,
    DateTime PublicationDate,
    string Platform,
    string? ContentType,
    string? Theme
) : ICommand<Result<CalendarEntryDto>>;

public record UpdateCalendarEntryCommand(
    Guid Id,
    DateTime? PublicationDate,
    string? Platform,
    string? ContentType,
    string? Theme
) : ICommand<Result<CalendarEntryDto>>;

public record DeleteCalendarEntryCommand(Guid Id) : ICommand<Result>;

public record ValidateCalendarEntryCommand(Guid Id) : ICommand<Result<CalendarEntryDto>>;
