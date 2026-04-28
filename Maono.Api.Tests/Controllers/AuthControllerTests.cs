using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Commands;
using Maono.Application.Features.Auth.Queries;
using Maono.Domain.Identity.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly AuthController _controller;

    private static readonly Guid _userId = Guid.NewGuid();

    public AuthControllerTests()
    {
        _mediatorMock    = new Mock<IMediator>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _controller = new AuthController(_mediatorMock.Object, _currentUserMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    // ── Register ──────────────────────────────────────────────

    [Fact]
    public async Task Register_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RegisterResponse>(new RegisterResponse(_userId, "a@b.com", "Test", Guid.NewGuid(), "WS", "FreelancerOwner")));

        var result = await _controller.Register(new SelfRegisterRequest("a@b.com", "Pass@1", "First", "Last", null, "MyWS"));

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RegisterResponse>("Email déjà utilisé"));

        var result = await _controller.Register(new SelfRegisterRequest("a@b.com", "Pass@1", "First", "Last", null, "MyWS"));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── RegisterByInvite ──────────────────────────────────────

    [Fact]
    public async Task RegisterByInvite_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterByInviteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RegisterResponse>(new RegisterResponse(_userId, "a@b.com", "Test", Guid.NewGuid(), "WS", "Planner")));

        var result = await _controller.RegisterByInvite(new InviteRegisterRequest("tok123", "Pass@1", "First", "Last", null));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task RegisterByInvite_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterByInviteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RegisterResponse>("Token invalide"));

        var result = await _controller.RegisterByInvite(new InviteRegisterRequest("bad", "Pass@1", "First", "Last", null));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── AdminCreateUser ───────────────────────────────────────

    [Fact]
    public async Task AdminCreateUser_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<AdminCreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RegisterResponse>(new RegisterResponse(_userId, "a@b.com", "Test", Guid.NewGuid(), "WS", "Strategist")));

        var result = await _controller.AdminCreateUser(new AdminCreateRequest("a@b.com", "F", "L", null, null, Guid.NewGuid(), "Strategist"));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task AdminCreateUser_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<AdminCreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RegisterResponse>("Workspace introuvable"));

        var result = await _controller.AdminCreateUser(new AdminCreateRequest("a@b.com", "F", "L", null, null, Guid.NewGuid(), "Strategist"));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Login ─────────────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<LoginResponse>(new LoginResponse("access", "refresh", _userId, "a@b.com", "Test", Guid.NewGuid(), Guid.NewGuid())));

        var result = await _controller.Login(new LoginRequest("a@b.com", "Pass@1"));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<LoginResponse>("Identifiants invalides"));

        var result = await _controller.Login(new LoginRequest("a@b.com", "wrong"));

        result.Should().BeOfType<UnauthorizedObjectResult>().Which.StatusCode.Should().Be(401);
    }

    // ── RefreshToken ──────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RefreshTokenResponse>(new RefreshTokenResponse("newAccess", "newRefresh", Guid.NewGuid())));

        var result = await _controller.RefreshToken(new RefreshTokenRequest("old", "refresh"));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturn401_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<RefreshTokenResponse>("Token expiré"));

        var result = await _controller.RefreshToken(new RefreshTokenRequest("bad", "bad"));

        result.Should().BeOfType<UnauthorizedObjectResult>().Which.StatusCode.Should().Be(401);
    }

    // ── GetSessions ───────────────────────────────────────────

    [Fact]
    public async Task GetSessions_ShouldReturn200_WhenSuccess()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(_userId);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetActiveSessionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<SessionDto>>(new List<SessionDto>()));

        var result = await _controller.GetSessions();

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetSessions_ShouldReturn401_WhenUserIdNull()
    {
        _currentUserMock.Setup(c => c.UserId).Returns((Guid?)null);

        var result = await _controller.GetSessions();

        result.Should().BeOfType<UnauthorizedObjectResult>().Which.StatusCode.Should().Be(401);
    }

    // ── RevokeSession ─────────────────────────────────────────

    [Fact]
    public async Task RevokeSession_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RevokeSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.RevokeSession(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RevokeSession_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RevokeSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Session introuvable"));

        var result = await _controller.RevokeSession(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── RevokeOtherSessions ───────────────────────────────────

    [Fact]
    public async Task RevokeOtherSessions_ShouldReturn200_WhenSuccess()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(_userId);
        _mediatorMock.Setup(m => m.Send(It.IsAny<RevokeAllOtherSessionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.RevokeOtherSessions(new RevokeOtherSessionsRequest(Guid.NewGuid()));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RevokeOtherSessions_ShouldReturn401_WhenUserIdNull()
    {
        _currentUserMock.Setup(c => c.UserId).Returns((Guid?)null);

        var result = await _controller.RevokeOtherSessions(new RevokeOtherSessionsRequest(Guid.NewGuid()));

        result.Should().BeOfType<UnauthorizedObjectResult>().Which.StatusCode.Should().Be(401);
    }
}
