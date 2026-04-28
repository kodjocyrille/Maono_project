using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Performance.DTOs;
using Maono.Application.Features.Performance.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class PerformanceControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PerformanceController _controller;

    public PerformanceControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PerformanceController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static PerformanceSnapshotDto MakeSnapshot() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 1000, 800, 200, 50, 0.05m);

    private static CampaignPerformanceSummaryDto MakeSummary() => new(
        Guid.NewGuid(), 5000, 4000, 1000, 200, 0.04m);

    [Fact]
    public async Task GetByPublication_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicationPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<PerformanceSnapshotDto>>(new List<PerformanceSnapshotDto> { MakeSnapshot() }));
        var result = await _controller.GetByPublication(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByPublication_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPublicationPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<PerformanceSnapshotDto>>("Erreur"));
        var result = await _controller.GetByPublication(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetByCampaign_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<CampaignPerformanceSummaryDto>>(new List<CampaignPerformanceSummaryDto> { MakeSummary() }));
        var result = await _controller.GetByCampaign(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByCampaign_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignPerformanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<CampaignPerformanceSummaryDto>>("Erreur"));
        var result = await _controller.GetByCampaign(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetCampaignSummary_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignPerformanceSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CampaignPerformanceSummaryDto>(MakeSummary()));
        var result = await _controller.GetCampaignSummary(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetCampaignSummary_ShouldReturn404_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignPerformanceSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CampaignPerformanceSummaryDto>("Erreur"));
        var result = await _controller.GetCampaignSummary(Guid.NewGuid());
        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
