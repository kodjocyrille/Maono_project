using Maono.Application.Common.Models;
using Maono.Application.Features.Messages.Queries;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Common;
using Maono.Domain.Content.Repository;
using Maono.Domain.Planning.Repository;
using MediatR;

namespace Maono.Application.Features.Messages.Handlers;

/// <summary>
/// ECR-008a — Export messages for all content items in a campaign.
/// Navigates: Campaign → CalendarEntry → ContentItem → ContentMessage
/// </summary>
public class ExportCampaignMessagesHandler : IRequestHandler<ExportCampaignMessagesQuery, Result<List<CampaignMessageExportDto>>>
{
    private readonly ICalendarRepository _calendarRepo;
    private readonly IContentRepository _contentRepo;
    private readonly IGenericRepository<ContentMessage> _messageRepo;

    public ExportCampaignMessagesHandler(ICalendarRepository calendarRepo, IContentRepository contentRepo, IGenericRepository<ContentMessage> messageRepo)
    {
        _calendarRepo = calendarRepo;
        _contentRepo = contentRepo;
        _messageRepo = messageRepo;
    }

    public async Task<Result<List<CampaignMessageExportDto>>> Handle(ExportCampaignMessagesQuery request, CancellationToken ct)
    {
        // Navigate Campaign → CalendarEntries → ContentItems
        var calendarEntries = await _calendarRepo.GetByCampaignAsync(request.CampaignId, ct);
        var entryIds = calendarEntries.Select(e => e.Id).ToHashSet();

        var contents = await _contentRepo.FindAsync(c => c.CalendarEntryId.HasValue && entryIds.Contains(c.CalendarEntryId.Value), ct);
        if (contents.Count == 0)
            return Result.Success(new List<CampaignMessageExportDto>());

        var contentMap = contents.ToDictionary(c => c.Id, c => c.Title);

        // Get all messages for those content items
        var allMessages = new List<CampaignMessageExportDto>();
        foreach (var contentId in contentMap.Keys)
        {
            var messages = await _messageRepo.FindAsync(m => m.ContentItemId == contentId, ct);
            allMessages.AddRange(messages.Select(m => new CampaignMessageExportDto(
                m.ContentItemId,
                contentMap.GetValueOrDefault(m.ContentItemId, "Unknown"),
                m.Id,
                m.AuthorType.ToString(),
                m.Body,
                m.SentAtUtc)));
        }

        return Result.Success(allMessages.OrderBy(m => m.SentAtUtc).ToList());
    }
}
