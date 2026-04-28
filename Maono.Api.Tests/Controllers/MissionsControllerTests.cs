using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Missions.Commands;
using Maono.Application.Features.Missions.DTOs;
using Maono.Application.Features.Missions.Queries;
using Maono.Domain.Missions.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class MissionsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MissionsController _controller;

    public MissionsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MissionsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static MissionDto MakeDto() => new(
        Guid.NewGuid(), "Mission A", MissionStatus.Brief, Guid.NewGuid(), null, null, null, DateTime.UtcNow);

    private static MissionDetailDto MakeDetail() => new(
        Guid.NewGuid(), "Mission A", null, MissionStatus.Brief, Guid.NewGuid(),
        null, null, null, DateTime.UtcNow, new List<MissionMemberDto>(), new List<MissionMilestoneDto>());

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateMissionCommand("Mission A", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<MissionDto>(MakeDto()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateMissionCommand("Mission A", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MissionDto>("Erreur"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListMissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<MissionDto>>(new List<MissionDto> { MakeDto() }));

        var result = await _controller.List();

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListMissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<MissionDto>>("Erreur"));

        var result = await _controller.List();

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMissionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<MissionDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMissionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MissionDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── Update ────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateMissionCommand(id, "Mission B", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Update(id, cmd);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateMissionCommand(id, "Mission B", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));

        var result = await _controller.Update(id, cmd);

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── UpdateStatus ──────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateMissionStatusCommand(id, MissionStatus.Closed);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<MissionDto>(MakeDto()));

        var result = await _controller.UpdateStatus(id, cmd);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateMissionStatusCommand(id, MissionStatus.Production);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<MissionDto>("Introuvable"));

        var result = await _controller.UpdateStatus(id, cmd);

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── Delete ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteMissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteMissionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
