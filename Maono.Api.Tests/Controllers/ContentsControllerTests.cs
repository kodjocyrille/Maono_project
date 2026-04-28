using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Content.Commands;
using Maono.Application.Features.Content.DTOs;
using Maono.Application.Features.Content.Queries;
using Maono.Domain.Content.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class ContentsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ContentsController _controller;

    public ContentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ContentsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static ContentItemDto MakeDto() => new(
        Guid.NewGuid(), "Titre", "Article", ContentStatus.Draft, null, 1, 1, DateTime.UtcNow);

    private static ContentItemDetailDto MakeDetail() => new(
        Guid.NewGuid(), "Titre", "Article", ContentStatus.Draft, null, 1, 1, null,
        DateTime.UtcNow, new List<BriefDto>(), new List<ChecklistItemDto>());

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateContentCommand("Titre", "Article", null, 1, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ContentItemDto>(MakeDto()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateContentCommand("Titre", null, null, 1, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ContentItemDto>("Erreur validation"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListContentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<ContentItemDto>>(new List<ContentItemDto> { MakeDto() }));

        var result = await _controller.List(null);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListContentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ContentItemDto>>("Erreur"));

        var result = await _controller.List("draft");

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ContentItemDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ContentItemDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── GetByDeadline ─────────────────────────────────────────

    [Fact]
    public async Task GetByDeadline_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentByDeadlineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<ContentItemDto>>(new List<ContentItemDto>()));

        var result = await _controller.GetByDeadline(DateTime.UtcNow);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByDeadline_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentByDeadlineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ContentItemDto>>("Erreur"));

        var result = await _controller.GetByDeadline(DateTime.UtcNow);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Update ────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateContentCommand(id, "Titre", null, 1, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Update(id, cmd);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        var cmd = new UpdateContentCommand(Guid.NewGuid(), "Titre", null, 1, null);

        var result = await _controller.Update(Guid.NewGuid(), cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── ChangeStatus ──────────────────────────────────────────

    [Fact]
    public async Task ChangeStatus_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ContentItemDto>(MakeDto()));

        var result = await _controller.ChangeStatus(Guid.NewGuid(), new ContentStatusRequest(ContentStatus.InReview));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ChangeStatus_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContentStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ContentItemDto>("Introuvable"));

        var result = await _controller.ChangeStatus(Guid.NewGuid(), new ContentStatusRequest(ContentStatus.Published));

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── Delete ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteContentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteContentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
