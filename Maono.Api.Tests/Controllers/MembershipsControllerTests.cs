using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.Commands;
using Maono.Application.Features.Workspaces.DTOs;
using Maono.Application.Features.Workspaces.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class MembershipsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MembershipsController _controller;

    public MembershipsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MembershipsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static MemberDto MakeMember() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Alice", "alice@test.com", "Planner", "Active", DateTime.UtcNow);

    private static InviteMemberResultDto MakeInviteResult() =>
        new(new MemberDto(Guid.NewGuid(), Guid.NewGuid(), "Alice", "alice@test.com", "Planner", "Invited", DateTime.UtcNow), "fake-invite-token");

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListMembersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<MemberDto>>(new List<MemberDto> { MakeMember() }));

        var result = await _controller.List(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListMembersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<MemberDto>>("Erreur"));

        var result = await _controller.List(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Invite ────────────────────────────────────────────────

    [Fact]
    public async Task Invite_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<InviteMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<InviteMemberResultDto>(MakeInviteResult()));

        var result = await _controller.Invite(Guid.NewGuid(), new InviteMemberRequest("alice@test.com", "Planner"));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Invite_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<InviteMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<InviteMemberResultDto>("Email déjà membre"));

        var result = await _controller.Invite(Guid.NewGuid(), new InviteMemberRequest("alice@test.com", "Planner"));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── UpdateRole ────────────────────────────────────────────

    [Fact]
    public async Task UpdateRole_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateMemberRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<MemberDto>(MakeMember()));

        var result = await _controller.UpdateRole(Guid.NewGuid(), Guid.NewGuid(), new UpdateRoleRequest("Strategist"));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateMemberRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MemberDto>("Rôle invalide"));

        var result = await _controller.UpdateRole(Guid.NewGuid(), Guid.NewGuid(), new UpdateRoleRequest("BadRole"));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Remove ────────────────────────────────────────────────

    [Fact]
    public async Task Remove_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RemoveMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Remove(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Remove_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RemoveMemberCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Membre introuvable"));

        var result = await _controller.Remove(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
