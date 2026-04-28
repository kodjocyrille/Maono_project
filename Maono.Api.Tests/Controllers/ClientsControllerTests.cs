using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Clients.Commands;
using Maono.Application.Features.Clients.DTOs;
using Maono.Application.Features.Clients.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class ClientsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ClientsController _controller;

    public ClientsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ClientsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static ClientDto MakeDto() => new(Guid.NewGuid(), "Acme", null, null, null, 0, DateTime.UtcNow);
    private static ClientDetailDto MakeDetail() => new(Guid.NewGuid(), "Acme", null, null, null, null, DateTime.UtcNow, new List<ClientContactDto>(), null);

    // ── Create ────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        var cmd = new CreateClientCommand("Acme", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ClientDto>(MakeDto()));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        var cmd = new CreateClientCommand("Acme", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ClientDto>("Erreur"));

        var result = await _controller.Create(cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── List ──────────────────────────────────────────────────

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListClientsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<ClientDto>>(new List<ClientDto> { MakeDto() }));

        var result = await _controller.List(null);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListClientsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<ClientDto>>("Erreur"));

        var result = await _controller.List("Acme");

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Get ───────────────────────────────────────────────────

    [Fact]
    public async Task Get_ShouldReturn200_WhenFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetClientByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ClientDetailDto>(MakeDetail()));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Get_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetClientByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ClientDetailDto>("Introuvable"));

        var result = await _controller.Get(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }

    // ── Update ────────────────────────────────────────────────

    [Fact]
    public async Task Update_ShouldReturn200_WhenSuccess()
    {
        var id = Guid.NewGuid();
        var cmd = new UpdateClientCommand(id, "Acme Nouveau", null, null, null, null);
        _mediatorMock.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<ClientDto>(MakeDto()));

        var result = await _controller.Update(id, cmd);

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        var cmd = new UpdateClientCommand(Guid.NewGuid(), "Acme", null, null, null, null);

        var result = await _controller.Update(Guid.NewGuid(), cmd);

        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    // ── Delete ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteClientCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}
