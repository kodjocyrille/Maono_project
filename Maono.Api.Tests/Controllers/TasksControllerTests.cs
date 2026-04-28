using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Tasks.Commands;
using Maono.Application.Features.Tasks.DTOs;
using Maono.Application.Features.Tasks.Queries;
using Maono.Domain.Content.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TasksController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    private static TaskDto MakeTask() => new(
        Guid.NewGuid(), Guid.NewGuid(), "Rédiger brief", null,
        ContentTaskStatus.Pending, ContentTaskPriority.Medium,
        null, null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task List_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListTasksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<TaskDto>>(new List<TaskDto> { MakeTask() }));
        var result = await _controller.List(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListTasksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<TaskDto>>("Contenu introuvable"));
        var result = await _controller.List(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<TaskDto>(MakeTask()));
        var request = new CreateTaskRequest("Rédiger brief", null, null, ContentTaskPriority.Medium, null);
        var result = await _controller.Create(Guid.NewGuid(), request);
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TaskDto>("Erreur"));
        var request = new CreateTaskRequest("", null, null, ContentTaskPriority.Low, null);
        var result = await _controller.Create(Guid.NewGuid(), request);
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<TaskDto>(MakeTask()));
        var request = new UpdateTaskRequest("Titre modifié", null, ContentTaskStatus.InProgress, null, null, null, null);
        var result = await _controller.Update(Guid.NewGuid(), request);
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<TaskDto>("Tâche introuvable"));
        var request = new UpdateTaskRequest(null, null, null, null, null, null, null);
        var result = await _controller.Update(Guid.NewGuid(), request);
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Delete_ShouldReturn200_WhenSuccess()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await _controller.Delete(Guid.NewGuid());
        result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ShouldReturn400_WhenFailure()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Introuvable"));
        var result = await _controller.Delete(Guid.NewGuid());
        result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}
