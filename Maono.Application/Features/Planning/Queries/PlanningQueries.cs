using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.DTOs;

namespace Maono.Application.Features.Planning.Queries;

public record ListCalendarEntriesQuery(Guid? CampaignId = null) : IQuery<Result<List<CalendarEntryDto>>>;
public record GetResourceCapacityQuery(DateTime WeekStart) : IQuery<Result<List<ResourceCapacityDto>>>;
