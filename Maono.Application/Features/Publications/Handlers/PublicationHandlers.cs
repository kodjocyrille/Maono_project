using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Publications.Commands;
using Maono.Application.Features.Publications.DTOs;
using Maono.Application.Features.Publications.Queries;
using Maono.Domain.Common;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Publications.Repository;
using MediatR;

namespace Maono.Application.Features.Publications.Handlers;

public class SchedulePublicationHandler : IRequestHandler<SchedulePublicationCommand, Result<PublicationDto>>
{
    private readonly IPublicationRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public SchedulePublicationHandler(IPublicationRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<PublicationDto>> Handle(SchedulePublicationCommand request, CancellationToken ct)
    {
        var pub = new Publication
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            ContentItemId = request.ContentItemId,
            Platform = request.Platform,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Status = PublicationStatus.Scheduled
        };
        await _repo.AddAsync(pub, ct);
        return Result.Success(new PublicationDto(pub.Id, pub.ContentItemId, pub.Platform, pub.Status, pub.ScheduledAtUtc, pub.CreatedAtUtc));
    }
}

public class PublishNowHandler : IRequestHandler<PublishNowCommand, Result<PublicationDto>>
{
    private readonly IPublicationRepository _repo;
    private readonly IGenericRepository<PublicationAttempt> _attemptRepo;

    public PublishNowHandler(IPublicationRepository repo, IGenericRepository<PublicationAttempt> attemptRepo)
    {
        _repo = repo;
        _attemptRepo = attemptRepo;
    }

    public async Task<Result<PublicationDto>> Handle(PublishNowCommand request, CancellationToken ct)
    {
        var pub = await _repo.GetByIdAsync(request.PublicationId, ct);
        if (pub == null) return Result.Failure<PublicationDto>("Publication not found", "NOT_FOUND");

        pub.Status = PublicationStatus.Published;
        pub.PublishedAtUtc = DateTime.UtcNow;
        _repo.Update(pub);

        // ECR-039 — Automatic publication log
        var existingAttempts = await _attemptRepo.FindAsync(a => a.PublicationId == pub.Id, ct);
        var attempt = new PublicationAttempt
        {
            WorkspaceId = pub.WorkspaceId,
            PublicationId = pub.Id,
            AttemptNumber = existingAttempts.Count + 1,
            StartedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = DateTime.UtcNow,
            Result = "Published"
        };
        await _attemptRepo.AddAsync(attempt, ct);

        return Result.Success(new PublicationDto(pub.Id, pub.ContentItemId, pub.Platform, pub.Status, pub.ScheduledAtUtc, pub.CreatedAtUtc));
    }
}

public class RetryPublicationHandler : IRequestHandler<RetryPublicationCommand, Result<PublicationDto>>
{
    private readonly IPublicationRepository _repo;
    public RetryPublicationHandler(IPublicationRepository repo) => _repo = repo;

    public async Task<Result<PublicationDto>> Handle(RetryPublicationCommand request, CancellationToken ct)
    {
        var pub = await _repo.GetByIdAsync(request.PublicationId, ct);
        if (pub == null) return Result.Failure<PublicationDto>("Publication not found", "NOT_FOUND");
        pub.Status = PublicationStatus.Scheduled;
        _repo.Update(pub);
        return Result.Success(new PublicationDto(pub.Id, pub.ContentItemId, pub.Platform, pub.Status, pub.ScheduledAtUtc, pub.CreatedAtUtc));
    }
}

public class GetPublicationByIdHandler : IRequestHandler<GetPublicationByIdQuery, Result<PublicationDetailDto>>
{
    private readonly IPublicationRepository _repo;
    public GetPublicationByIdHandler(IPublicationRepository repo) => _repo = repo;

    public async Task<Result<PublicationDetailDto>> Handle(GetPublicationByIdQuery request, CancellationToken ct)
    {
        var pub = await _repo.GetWithDetailsAsync(request.Id, ct);
        if (pub == null) return Result.Failure<PublicationDetailDto>("Publication not found", "NOT_FOUND");
        var attempts = pub.Attempts.Select(a => new PublicationAttemptDto(a.Id, a.AttemptNumber, a.StartedAtUtc, a.Result, a.ErrorMessage)).ToList();
        return Result.Success(new PublicationDetailDto(pub.Id, pub.ContentItemId, pub.Platform, pub.Status, pub.ScheduledAtUtc, pub.PublishedAtUtc, pub.ExternalPostId, pub.CreatedAtUtc, attempts));
    }
}

public class ListPublicationsHandler : IRequestHandler<ListPublicationsQuery, Result<List<PublicationDto>>>
{
    private readonly IPublicationRepository _repo;
    public ListPublicationsHandler(IPublicationRepository repo) => _repo = repo;

    public async Task<Result<List<PublicationDto>>> Handle(ListPublicationsQuery request, CancellationToken ct)
    {
        IReadOnlyList<Publication> pubs;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<PublicationStatus>(request.Status, true, out var status))
            pubs = await _repo.GetByStatusAsync(status, ct);
        else
            pubs = await _repo.GetAllAsync(ct);

        var dtos = pubs.Select(p => new PublicationDto(p.Id, p.ContentItemId, p.Platform, p.Status, p.ScheduledAtUtc, p.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

public class DeletePublicationHandler : IRequestHandler<DeletePublicationCommand, Result>
{
    private readonly IPublicationRepository _repo;
    public DeletePublicationHandler(IPublicationRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeletePublicationCommand request, CancellationToken ct)
    {
        var pub = await _repo.GetByIdAsync(request.Id, ct);
        if (pub == null) return Result.Failure("Publication not found", "NOT_FOUND");
        _repo.Remove(pub);
        return Result.Success();
    }
}

