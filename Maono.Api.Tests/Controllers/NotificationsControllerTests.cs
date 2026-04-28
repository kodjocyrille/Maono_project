using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Notifications.Commands;
using Maono.Application.Features.Notifications.DTOs;
using Maono.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new NotificationsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static NotificationDto MakeNotif() => new(
        Guid.NewGuid(), "content_approved", "Approuvé", "Votre contenu a été approuvé.",
        "sent", DateTime.UtcNow, null, DateTime.UtcNow);

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<NotificationDto>>(new List<NotificationDto> { MakeNotif() }));
        var result = await _controller.List();
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<NotificationDto>>("Erreur"));
        var result = await _controller.List();
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task MarkRead_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkNotificationReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await _controller.MarkRead(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task MarkRead_ShouldReturn404_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkNotificationReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));
        var result = await _controller.MarkRead(Guid.NewGuid());
        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task MarkAllRead_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkAllNotificationsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await _controller.MarkAllRead();
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task MarkAllRead_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkAllNotificationsReadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Erreur"));
        var result = await _controller.MarkAllRead();
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
