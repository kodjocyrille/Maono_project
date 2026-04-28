using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Portal.Commands;
using Maono.Application.Features.Portal.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class PortalControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PortalController _controller;
    private readonly PortalTokensController _tokensController;

    public PortalControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PortalController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        _tokensController = new PortalTokensController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task GetContents_ShouldReturn200_WhenValid()
    {
        var view = new PortalViewDto(Guid.NewGuid(), "Acme", new List<PortalContentItemDto>(), DateTime.UtcNow.AddDays(3));
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPortalContentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PortalViewDto>(view));
        var result = await _controller.GetContents("valid-token");
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetContents_ShouldReturn401_WhenTokenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPortalContentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PortalViewDto>("Token invalide"));
        var result = await _controller.GetContents("bad-token");
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task SubmitDecision_ShouldReturn200_WhenValid()
    {
        var dto = new PortalDecisionDto(Guid.NewGuid(), "approved", null, DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SubmitPortalDecisionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PortalDecisionDto>(dto));
        var result = await _controller.SubmitDecision("tok", new PortalDecisionRequest(Guid.NewGuid(), "approved", null));
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SubmitDecision_ShouldReturn400_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<SubmitPortalDecisionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PortalDecisionDto>("Token expiré"));
        var result = await _controller.SubmitDecision("bad", new PortalDecisionRequest(Guid.NewGuid(), "approved", null));
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Generate_ShouldReturn201_WhenValid()
    {
        var tokenDto = new PortalTokenDto(Guid.NewGuid(), "tok123", "https://portal/tok123", DateTime.UtcNow.AddDays(3));
        _mediatorMock.Setup(m => m.Send(It.IsAny<GeneratePortalTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PortalTokenDto>(tokenDto));
        var result = await _tokensController.Generate(Guid.NewGuid(), new GenerateTokenRequest(null, null, 72));
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Generate_ShouldReturn400_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GeneratePortalTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PortalTokenDto>("Client introuvable"));
        var result = await _tokensController.Generate(Guid.NewGuid(), new GenerateTokenRequest(null, null, 72));
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Revoke_ShouldReturn200_WhenValid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RevokePortalTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await _tokensController.Revoke(Guid.NewGuid(), Guid.NewGuid(), null);
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Revoke_ShouldReturn400_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RevokePortalTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Token introuvable"));
        var result = await _tokensController.Revoke(Guid.NewGuid(), Guid.NewGuid(), null);
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
