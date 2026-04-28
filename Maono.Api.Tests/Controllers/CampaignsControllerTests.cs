using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.Commands;
using Maono.Application.Features.Campaigns.DTOs;
using Maono.Application.Features.Campaigns.Queries;
using Maono.Domain.Campaigns.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class CampaignsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CampaignsController _controller;

    public CampaignsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CampaignsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static CampaignDto MakeDto() => new(
        Guid.NewGuid(), "Camp", null, null, CampaignStatus.Active,
        Guid.NewGuid(), null, null, null, null, DateTime.UtcNow);

    private static CampaignDetailDto MakeDetail() => new(
        Guid.NewGuid(), "Camp", null, null, CampaignStatus.Active,
        Guid.NewGuid(), null, null, null, null,
        null, null, null, null, null,
        DateTime.UtcNow, new List<CampaignKpiDto>());

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateCampaignCommand("Camp", null, null, Guid.NewGuid(), null, null, null, null, null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CampaignDto>(MakeDto()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateCampaignCommand("Camp", null, null, Guid.NewGuid(), null, null, null, null, null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CampaignDto>("Client introuvable"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<CampaignSummaryDto>>(new List<CampaignSummaryDto> { new CampaignSummaryDto(Guid.NewGuid(), "Camp", CampaignStatus.Active, null) }));

        var result = await _controller.List(null);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListCampaignsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<CampaignSummaryDto>>("Erreur"));

        var result = await _controller.List(null);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CampaignDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCampaignByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CampaignDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
