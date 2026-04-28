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

public class WorkspacesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly WorkspacesController _controller;

    public WorkspacesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new WorkspacesController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static WorkspaceDto MakeDto() =>
        new(Guid.NewGuid(), "Test WS", "test-ws", "Free", "UTC", null, DateTime.UtcNow);

    private static WorkspaceDetailDto MakeDetail() =>
        new(Guid.NewGuid(), "Test WS", "test-ws", "Free", "UTC", null, null, DateTime.UtcNow, new List<WorkspaceMemberDto>());

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateWorkspaceCommand("WS", "ws-slug", null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<WorkspaceDto>(MakeDto()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateWorkspaceCommand("WS", "ws-slug", null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<WorkspaceDto>("Slug déjà utilisé"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListWorkspacesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<WorkspaceDto>>(new List<WorkspaceDto> { MakeDto() }));

        var result = await _controller.List();

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListWorkspacesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<WorkspaceDto>>("Erreur"));

        var result = await _controller.List();

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetWorkspaceByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<WorkspaceDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetWorkspaceByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<WorkspaceDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── UpdateSettings ────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateWorkspaceSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<WorkspaceDto>(MakeDto()));

        var result = await _controller.UpdateSettings(Guid.NewGuid(), new UpdateSettingsRequest(null, "Europe/Paris", null));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateWorkspaceSettingsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<WorkspaceDto>("Workspace introuvable"));

        var result = await _controller.UpdateSettings(Guid.NewGuid(), new UpdateSettingsRequest(null, null, null));

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
