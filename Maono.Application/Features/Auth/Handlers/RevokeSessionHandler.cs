using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Commands;
using MediatR;

namespace Maono.Application.Features.Auth.Handlers;

public class RevokeSessionHandler : IRequestHandler<RevokeSessionCommand, Result>
{
    private readonly IAuthenticationService _authService;

    public RevokeSessionHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        await _authService.RevokeSessionAsync(request.SessionId, cancellationToken);
        return Result.Success();
    }
}

public class RevokeAllOtherSessionsHandler : IRequestHandler<RevokeAllOtherSessionsCommand, Result>
{
    private readonly IAuthenticationService _authService;

    public RevokeAllOtherSessionsHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(RevokeAllOtherSessionsCommand request, CancellationToken cancellationToken)
    {
        await _authService.RevokeAllSessionsExceptAsync(request.UserId, request.CurrentSessionId, cancellationToken);
        return Result.Success();
    }
}
