using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.Commands;
using Maono.Application.Features.Planning.DTOs;
using Maono.Application.Features.Planning.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class CalendarControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CalendarController _controller;

    public CalendarControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CalendarController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CalendarEntryDto MakeEntry() => new(
        Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(3),
        "Instagram", "Post", "Thème", "Draft", DateTime.UtcNow);

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListCalendarEntriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<CalendarEntryDto>>(new List<CalendarEntryDto> { MakeEntry() }));

        var result = await _controller.List(null);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListCalendarEntriesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<CalendarEntryDto>>("Erreur"));

        var result = await _controller.List(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── GetCapacity ───────────────────────────────────────────

    [Fact]
    public async Task GetCapacity_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetResourceCapacityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<ResourceCapacityDto>>(new List<ResourceCapacityDto>()));

        var result = await _controller.GetCapacity(DateTime.UtcNow);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetCapacity_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetResourceCapacityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ResourceCapacityDto>>("Erreur"));

        var result = await _controller.GetCapacity(DateTime.UtcNow);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateCalendarEntryCommand(Guid.NewGuid(), DateTime.UtcNow.AddDays(3), "Instagram", null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CalendarEntryDto>(MakeEntry()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateCalendarEntryCommand(Guid.NewGuid(), DateTime.UtcNow, "Instagram", null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CalendarEntryDto>("Campagne introuvable"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Update ────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CalendarEntryDto>(MakeEntry()));

        var result = await _controller.Update(id, new UpdateCalendarEntryCommand(id, null, "Twitter", null, null));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenFailure()
    {
        var id = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CalendarEntryDto>("Introuvable"));

        var result = await _controller.Update(id, new UpdateCalendarEntryCommand(id, null, null, null, null));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Delete ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Validate ──────────────────────────────────────────────

    [Fact]
    public async Task Validate_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ValidateCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CalendarEntryDto>(MakeEntry()));

        var result = await _controller.Validate(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Validate_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ValidateCalendarEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CalendarEntryDto>("Introuvable"));

        var result = await _controller.Validate(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
