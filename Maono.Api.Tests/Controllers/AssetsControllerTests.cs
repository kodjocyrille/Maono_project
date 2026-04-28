using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.Commands;
using Maono.Application.Features.Assets.DTOs;
using Maono.Application.Features.Assets.Queries;
using Maono.Domain.Assets.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class AssetsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AssetsController _controller;

    public AssetsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AssetsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static AssetDetailDto MakeDetail() => new(
        Guid.NewGuid(), Guid.NewGuid(), AssetType.Image, 1,
        "/path/file.jpg", "image/jpeg", "file.jpg", AssetVisibility.Internal,
        null, DateTime.UtcNow, new List<AssetVersionDto>());

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAssetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AssetDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAssetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AssetDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── GetVersions ───────────────────────────────────────────

    [Fact]
    public async Task GetVersions_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAssetVersionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<AssetVersionDto>>(new List<AssetVersionDto>()));

        var result = await _controller.GetVersions(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetVersions_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAssetVersionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<AssetVersionDto>>("Introuvable"));

        var result = await _controller.GetVersions(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── Restore ───────────────────────────────────────────────

    [Fact]
    public async Task Restore_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreAssetVersionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AssetDto>(new AssetDto(Guid.NewGuid(), Guid.NewGuid(), AssetType.Image, 2, "image/jpeg", "file.jpg", null, DateTime.UtcNow)));

        var result = await _controller.Restore(Guid.NewGuid(), new RestoreVersionRequest(2));

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Restore_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreAssetVersionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AssetDto>("Version introuvable"));

        var result = await _controller.Restore(Guid.NewGuid(), new RestoreVersionRequest(99));

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── InitiateUpload ────────────────────────────────────────

    [Fact]
    public async Task InitiateUpload_ShouldReturn201_WhenSuccess()
    {
        var cmd = new InitiateUploadSessionCommand(Guid.NewGuid(), "file.jpg", "image/jpeg", 1024, "abc123sha256");
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UploadSessionDto>(new UploadSessionDto(Guid.NewGuid(), "https://minio.url/presigned", DateTime.UtcNow.AddMinutes(15))));

        var result = await _controller.InitiateUpload(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task InitiateUpload_ShouldReturn400_WhenFailure()
    {
        var cmd = new InitiateUploadSessionCommand(Guid.NewGuid(), "file.jpg", "image/jpeg", 1024, "abc123sha256");
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UploadSessionDto>("Erreur initiation"));

        var result = await _controller.InitiateUpload(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── GetUploadSession ──────────────────────────────────────

    [Fact]
    public async Task GetUploadSession_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUploadSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UploadSessionDto>(new UploadSessionDto(Guid.NewGuid(), "https://url", DateTime.UtcNow.AddMinutes(10))));

        var result = await _controller.GetUploadSession(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetUploadSession_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUploadSessionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UploadSessionDto>("Session expirée"));

        var result = await _controller.GetUploadSession(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── ConfirmUpload ─────────────────────────────────────────

    [Fact]
    public async Task ConfirmUpload_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmUploadSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<AssetUploadConfirmedDto>(new AssetUploadConfirmedDto(Guid.NewGuid(), "/assets/file.jpg", 1, "file.jpg")));

        var result = await _controller.ConfirmUpload(Guid.NewGuid(), new ConfirmUploadRequest(1024, "sha256abc"));

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task ConfirmUpload_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmUploadSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AssetUploadConfirmedDto>("SHA-256 mismatch"));

        var result = await _controller.ConfirmUpload(Guid.NewGuid(), new ConfirmUploadRequest(1024, "wrongsha"));

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
