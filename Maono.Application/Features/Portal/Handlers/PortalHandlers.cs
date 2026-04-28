using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Approvals.Commands;
using Maono.Application.Features.Approvals.DTOs;
using Maono.Application.Features.Portal.Commands;
using Maono.Application.Features.Portal.Queries;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Approval.Repository;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Portal.Handlers;

public class GeneratePortalTokenHandler : IRequestHandler<GeneratePortalTokenCommand, Result<PortalTokenDto>>
{
    private readonly IPortalAccessTokenRepository _tokenRepo;
    private readonly ICurrentUserService _currentUser;

    public GeneratePortalTokenHandler(IPortalAccessTokenRepository tokenRepo, ICurrentUserService currentUser)
    {
        _tokenRepo = tokenRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<PortalTokenDto>> Handle(GeneratePortalTokenCommand request, CancellationToken ct)
    {
        // Revoke any existing active tokens for the same client + content scope
        var existing = await _tokenRepo.GetByClientAsync(request.ClientOrganizationId, ct);
        foreach (var token in existing.Where(t => t.IsActive && t.ContentItemId == request.ContentItemId))
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = "Remplacé par un nouveau token.";
            _tokenRepo.Update(token);
        }

        // Generate cryptographically random URL-safe token
        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var portalToken = new PortalAccessToken
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            ClientOrganizationId = request.ClientOrganizationId,
            ContentItemId = request.ContentItemId,
            CampaignId = request.CampaignId,
            Token = rawToken,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(request.ExpiryHours),
            CreatedByUserId = _currentUser.UserId!.Value
        };
        await _tokenRepo.AddAsync(portalToken, ct);

        // Portal URL would come from config in production
        var portalUrl = $"/portal/view?token={rawToken}";

        return Result.Success(new PortalTokenDto(
            portalToken.Id, rawToken, portalUrl, portalToken.ExpiresAtUtc));
    }
}

public class RevokePortalTokenHandler : IRequestHandler<RevokePortalTokenCommand, Result>
{
    private readonly IPortalAccessTokenRepository _tokenRepo;
    public RevokePortalTokenHandler(IPortalAccessTokenRepository tokenRepo) => _tokenRepo = tokenRepo;

    public async Task<Result> Handle(RevokePortalTokenCommand request, CancellationToken ct)
    {
        var token = await _tokenRepo.GetByIdAsync(request.TokenId, ct);
        if (token == null) return Result.Failure("Token introuvable.", "NOT_FOUND");
        if (!token.IsActive) return Result.Failure("Ce token n'est plus actif.", "ALREADY_INACTIVE");

        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedReason = request.Reason ?? "Révoqué manuellement.";
        _tokenRepo.Update(token);
        return Result.Success();
    }
}

public class GetPortalContentsHandler : IRequestHandler<GetPortalContentsQuery, Result<PortalViewDto>>
{
    private readonly IPortalAccessTokenRepository _tokenRepo;
    private readonly IContentRepository _contentRepo;
    private readonly IAssetStorageService _storage;

    public GetPortalContentsHandler(
        IPortalAccessTokenRepository tokenRepo,
        IContentRepository contentRepo,
        IAssetStorageService storage)
    {
        _tokenRepo = tokenRepo;
        _contentRepo = contentRepo;
        _storage = storage;
    }

    public async Task<Result<PortalViewDto>> Handle(GetPortalContentsQuery request, CancellationToken ct)
    {
        var tokenEntity = await _tokenRepo.GetByTokenAsync(request.Token, ct);
        if (tokenEntity == null || !tokenEntity.IsActive)
            return Result.Failure<PortalViewDto>("Token invalide ou expiré.", "TOKEN_INVALID");

        // Get contents for the client (filtered by ContentItemId if scoped)
        List<Domain.Content.Entities.ContentItem> contents;
        if (tokenEntity.ContentItemId.HasValue)
        {
            var item = await _contentRepo.GetByIdAsync(tokenEntity.ContentItemId.Value, ct);
            contents = item != null ? new List<Domain.Content.Entities.ContentItem> { item } : new();
        }
        else
        {
            // Return all content in workspace (in production, filter by client's campaigns)
            var all = await _contentRepo.GetAllAsync(ct);
            contents = all.ToList();
        }

        var contentDtos = contents.Select(c => new PortalContentItemDto(
            c.Id,
            c.Title,
            c.Status.ToString(),
            c.Briefs.FirstOrDefault()?.Body,
            new List<PortalAssetDto>(),  // Assets loaded via separate query in v2
            null)).ToList();

        return Result.Success(new PortalViewDto(
            tokenEntity.ClientOrganizationId,
            "Client",  // Name would be loaded from client repo
            contentDtos,
            tokenEntity.ExpiresAtUtc));
    }
}

public class SubmitPortalDecisionHandler : IRequestHandler<SubmitPortalDecisionCommand, Result<PortalDecisionDto>>
{
    private readonly IPortalAccessTokenRepository _tokenRepo;
    private readonly IMediator _mediator;

    public SubmitPortalDecisionHandler(IPortalAccessTokenRepository tokenRepo, IMediator mediator)
    {
        _tokenRepo = tokenRepo;
        _mediator = mediator;
    }

    public async Task<Result<PortalDecisionDto>> Handle(SubmitPortalDecisionCommand request, CancellationToken ct)
    {
        var tokenEntity = await _tokenRepo.GetByTokenAsync(request.Token, ct);
        if (tokenEntity == null || !tokenEntity.IsActive)
            return Result.Failure<PortalDecisionDto>("Token invalide ou expiré.", "TOKEN_INVALID");

        // Validate that the content is in scope for this token
        if (tokenEntity.ContentItemId.HasValue && tokenEntity.ContentItemId != request.ContentItemId)
            return Result.Failure<PortalDecisionDto>("Ce contenu n'est pas accessible avec ce token.", "OUT_OF_SCOPE");

        var decisionStr = request.Decision.ToLower();
        if (decisionStr is not ("approved" or "changes_requested"))
            return Result.Failure<PortalDecisionDto>("Décision invalide. Valeurs acceptées : 'approved', 'changes_requested'.", "INVALID_DECISION");

        var approvalStatus = decisionStr == "approved" ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        var approvalResult = await _mediator.Send(
            new SubmitClientApprovalCommand(request.ContentItemId, approvalStatus, request.Comment), ct);

        if (!approvalResult.IsSuccess)
            return Result.Failure<PortalDecisionDto>(approvalResult.Error!, "APPROVAL_FAILED");

        return Result.Success(new PortalDecisionDto(
            request.ContentItemId, request.Decision, request.Comment, DateTime.UtcNow));
    }
}
