using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Messages.Commands;
using Maono.Application.Features.Messages.DTOs;
using Maono.Application.Features.Messages.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class MessagesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MessagesController _controller;

    public MessagesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MessagesController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static ContentMessageDto MakeMsg() => new(
        Guid.NewGuid(), Guid.NewGuid(), "InternalUser", Guid.NewGuid(), "Bonjour !", DateTime.UtcNow);

    // ── GetByContent ──────────────────────────────────────────

    [Fact]
    public async Task GetByContent_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentMessagesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<ContentMessageDto>>(new List<ContentMessageDto> { MakeMsg() }));

        var result = await _controller.GetByContent(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByContent_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetContentMessagesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ContentMessageDto>>("Contenu introuvable"));

        var result = await _controller.GetByContent(Guid.NewGuid());

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Send ──────────────────────────────────────────────────

    [Fact]
    public async Task Send_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<SendContentMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ContentMessageDto>(MakeMsg()));
        var result = await _controller.Send(Guid.NewGuid(), new SendMessageRequest("Bonjour !"));
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Send_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<SendContentMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ContentMessageDto>("Message vide"));
        var result = await _controller.Send(Guid.NewGuid(), new SendMessageRequest(""));
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
