using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Publications.Commands;
using Maono.Application.Features.Publications.DTOs;
using Maono.Application.Features.Publications.Queries;
using Maono.Domain.Publications.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class PublicationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PublicationsController _controller;

    public PublicationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PublicationsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static PublicationDto MakeDto() => new(
        Guid.NewGuid(), Guid.NewGuid(), SocialPlatform.Instagram, PublicationStatus.Scheduled,
        DateTime.UtcNow.AddDays(1), DateTime.UtcNow);

    private static PublicationDetailDto MakeDetail() => new(
        Guid.NewGuid(), Guid.NewGuid(), SocialPlatform.Instagram, PublicationStatus.Scheduled,
        DateTime.UtcNow.AddDays(1), null, null, DateTime.UtcNow, new List<PublicationAttemptDto>());

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListPublicationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<PublicationDto>>(new List<PublicationDto> { MakeDto() }));
        var result = await _controller.List(null);
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListPublicationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<PublicationDto>>("Erreur"));
        var result = await _controller.List("scheduled");
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PublicationDetailDto>(MakeDetail()));
        var result = await _controller.Get(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicationByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PublicationDetailDto>("Introuvable"));
        var result = await _controller.Get(Guid.NewGuid());
        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Schedule_ShouldReturn201_WhenSuccess()
    {
        var cmd = new SchedulePublicationCommand(Guid.NewGuid(), SocialPlatform.Instagram, DateTime.UtcNow.AddDays(1));
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PublicationDto>(MakeDto()));
        var result = await _controller.Schedule(cmd);
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Schedule_ShouldReturn400_WhenFailure()
    {
        var cmd = new SchedulePublicationCommand(Guid.NewGuid(), SocialPlatform.Instagram, DateTime.UtcNow.AddDays(1));
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PublicationDto>("Contenu introuvable"));
        var result = await _controller.Schedule(cmd);
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PublishNow_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<PublishNowCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PublicationDto>(MakeDto()));
        var result = await _controller.PublishNow(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task PublishNow_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<PublishNowCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PublicationDto>("Erreur publication"));
        var result = await _controller.PublishNow(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Retry_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RetryPublicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PublicationDto>(MakeDto()));
        var result = await _controller.Retry(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Retry_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RetryPublicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PublicationDto>("Impossible de relancer"));
        var result = await _controller.Retry(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeletePublicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await _controller.Delete(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeletePublicationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));
        var result = await _controller.Delete(Guid.NewGuid());
        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
