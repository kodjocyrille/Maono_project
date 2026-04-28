using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Queries;
using MediatR;

namespace Maono.Application.Features.Auth.Handlers;

public class GetActiveSessionsHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<SessionDto>>>
{
    private readonly IAuthenticationService _authService;

    public GetActiveSessionsHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<List<SessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _authService.GetActiveSessionsAsync(request.UserId, cancellationToken);
        return Result.Success(sessions);
    }
}
