using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Messages.Commands;
using Maono.Application.Features.Messages.DTOs;
using Maono.Application.Features.Messages.Queries;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Approval.Repository;
using MediatR;

namespace Maono.Application.Features.Messages.Handlers;

public class GetContentMessagesHandler : IRequestHandler<GetContentMessagesQuery, Result<List<ContentMessageDto>>>
{
    private readonly IContentMessageRepository _repo;
    public GetContentMessagesHandler(IContentMessageRepository repo) => _repo = repo;

    public async Task<Result<List<ContentMessageDto>>> Handle(GetContentMessagesQuery request, CancellationToken ct)
    {
        var messages = await _repo.GetByContentItemAsync(request.ContentItemId, ct);
        var dtos = messages.Select(m => new ContentMessageDto(m.Id, m.ContentItemId, m.AuthorType.ToString(), m.AuthorId, m.Body, m.SentAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

public class SendContentMessageHandler : IRequestHandler<SendContentMessageCommand, Result<ContentMessageDto>>
{
    private readonly IContentMessageRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public SendContentMessageHandler(IContentMessageRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<ContentMessageDto>> Handle(SendContentMessageCommand request, CancellationToken ct)
    {
        var msg = new ContentMessage
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            ContentItemId = request.ContentItemId,
            AuthorType = ActorType.InternalUser,
            AuthorId = _currentUser.UserId,
            Body = request.Body
        };
        await _repo.AddAsync(msg, ct);
        return Result.Success(new ContentMessageDto(msg.Id, msg.ContentItemId, msg.AuthorType.ToString(), msg.AuthorId, msg.Body, msg.SentAtUtc));
    }
}
