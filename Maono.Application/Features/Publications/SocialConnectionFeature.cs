using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Common;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Publications.Enums;
using MediatR;

namespace Maono.Application.Features.Publications;

// ── ECR-010 — Social Connection Management ──────────────

public record ConnectSocialAccountCommand(
    SocialPlatform Platform,
    string ExternalAccountId,
    string? AccountName,
    string? AccessTokenRef,
    string? RefreshTokenRef
) : ICommand<Result<SocialConnectionDto>>;

public record DisconnectSocialAccountCommand(Guid ConnectionId) : ICommand<Result>;
public record ListSocialConnectionsQuery() : IQuery<Result<List<SocialConnectionDto>>>;

public record SocialConnectionDto(
    Guid Id,
    string Platform,
    string ExternalAccountId,
    string? AccountName,
    string? Status,
    DateTime? ConnectedAtUtc,
    DateTime? TokenExpiresAtUtc
);

public class ConnectSocialAccountHandler : IRequestHandler<ConnectSocialAccountCommand, Result<SocialConnectionDto>>
{
    private readonly IGenericRepository<SocialConnection> _connRepo;
    private readonly ICurrentUserService _currentUser;

    public ConnectSocialAccountHandler(IGenericRepository<SocialConnection> connRepo, ICurrentUserService currentUser)
    {
        _connRepo = connRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<SocialConnectionDto>> Handle(ConnectSocialAccountCommand request, CancellationToken ct)
    {
        // Check for duplicate
        var existing = await _connRepo.FirstOrDefaultAsync(
            c => c.Platform == request.Platform && c.ExternalAccountId == request.ExternalAccountId, ct);
        if (existing != null)
            return Result.Failure<SocialConnectionDto>("Ce compte social est déjà connecté.", "DUPLICATE");

        var conn = new SocialConnection
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Platform = request.Platform,
            ExternalAccountId = request.ExternalAccountId,
            AccountName = request.AccountName,
            AccessTokenRef = request.AccessTokenRef,
            RefreshTokenRef = request.RefreshTokenRef,
            Status = "Connected",
            ConnectedAtUtc = DateTime.UtcNow
        };
        await _connRepo.AddAsync(conn, ct);

        return Result.Success(MapToDto(conn));
    }

    private static SocialConnectionDto MapToDto(SocialConnection c) => new(
        c.Id, c.Platform.ToString(), c.ExternalAccountId, c.AccountName,
        c.Status, c.ConnectedAtUtc, c.TokenExpiresAtUtc);
}

public class DisconnectSocialAccountHandler : IRequestHandler<DisconnectSocialAccountCommand, Result>
{
    private readonly IGenericRepository<SocialConnection> _connRepo;
    public DisconnectSocialAccountHandler(IGenericRepository<SocialConnection> connRepo) => _connRepo = connRepo;

    public async Task<Result> Handle(DisconnectSocialAccountCommand request, CancellationToken ct)
    {
        var conn = await _connRepo.GetByIdAsync(request.ConnectionId, ct);
        if (conn == null) return Result.Failure("Connexion sociale introuvable.", "NOT_FOUND");
        _connRepo.Remove(conn);
        return Result.Success();
    }
}

public class ListSocialConnectionsHandler : IRequestHandler<ListSocialConnectionsQuery, Result<List<SocialConnectionDto>>>
{
    private readonly IGenericRepository<SocialConnection> _connRepo;
    public ListSocialConnectionsHandler(IGenericRepository<SocialConnection> connRepo) => _connRepo = connRepo;

    public async Task<Result<List<SocialConnectionDto>>> Handle(ListSocialConnectionsQuery request, CancellationToken ct)
    {
        var connections = await _connRepo.FindAsync(_ => true, ct);
        return Result.Success(connections.Select(c => new SocialConnectionDto(
            c.Id, c.Platform.ToString(), c.ExternalAccountId, c.AccountName,
            c.Status, c.ConnectedAtUtc, c.TokenExpiresAtUtc)).ToList());
    }
}
