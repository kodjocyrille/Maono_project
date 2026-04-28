using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.DTOs;
using Maono.Application.Features.Planning.Queries;
using Maono.Domain.Common;
using Maono.Domain.Content.Entities;
using Maono.Domain.Planning.Repository;
using MediatR;

namespace Maono.Application.Features.Planning.Handlers;

public class ListCalendarEntriesHandler : IRequestHandler<ListCalendarEntriesQuery, Result<List<CalendarEntryDto>>>
{
    private readonly ICalendarRepository _repo;
    public ListCalendarEntriesHandler(ICalendarRepository repo) => _repo = repo;

    public async Task<Result<List<CalendarEntryDto>>> Handle(ListCalendarEntriesQuery request, CancellationToken ct)
    {
        var entries = request.CampaignId.HasValue
            ? await _repo.GetByCampaignAsync(request.CampaignId.Value, ct)
            : await _repo.GetAllAsync(ct);

        var dtos = entries.Select(e => new CalendarEntryDto(
            e.Id, e.CampaignId, e.PublicationDate, e.Platform.ToString(), e.ContentType, e.Theme, e.Status, e.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

/// <summary>
/// ECR-009 — Compute resource capacity from assigned ContentTasks for the requested week.
/// Groups tasks by assignee and calculates assigned hours (1 task = 2h estimate).
/// Default capacity: 40h/week. Overload threshold: 80%.
/// </summary>
public class GetResourceCapacityHandler : IRequestHandler<GetResourceCapacityQuery, Result<List<ResourceCapacityDto>>>
{
    private readonly IGenericRepository<ContentTask> _taskRepo;
    private const decimal DefaultCapacityHours = 40m;
    private const decimal DefaultOverloadThreshold = 0.8m;
    private const decimal HoursPerTask = 2m;

    public GetResourceCapacityHandler(IGenericRepository<ContentTask> taskRepo) => _taskRepo = taskRepo;

    public async Task<Result<List<ResourceCapacityDto>>> Handle(GetResourceCapacityQuery request, CancellationToken ct)
    {
        // Determine the week boundaries
        var weekStart = request.WeekStart.Date;
        var weekEnd = weekStart.AddDays(7);

        // Get all tasks with a DueDate in the requested week that are assigned
        var tasks = await _taskRepo.FindAsync(t =>
            t.AssignedToUserId.HasValue &&
            t.DueDate.HasValue &&
            t.DueDate.Value >= weekStart &&
            t.DueDate.Value < weekEnd &&
            t.Status != ContentTaskStatus.Completed, ct);

        // Group by user
        var grouped = tasks
            .GroupBy(t => t.AssignedToUserId!.Value)
            .Select(g => new ResourceCapacityDto(
                Guid.NewGuid(),
                g.Key,
                weekStart,
                DefaultCapacityHours,
                g.Count() * HoursPerTask,
                DefaultCapacityHours * DefaultOverloadThreshold))
            .ToList();

        return Result.Success(grouped);
    }
}

