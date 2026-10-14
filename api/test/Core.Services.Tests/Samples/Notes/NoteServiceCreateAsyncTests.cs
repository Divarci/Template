using AwesomeAssertions;
using Core.Domain.Entities.Samples.Notes;
using Core.Domain.Entities.Samples.Notes.Models;
using Core.Domain.Entities.System.AuditLogs;
using Core.Library.ResultPattern;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace Core.Services.Tests.Samples.Notes;

public sealed class NoteServiceCreateAsyncTests
{
    [Fact]
    public async Task CreateAsync_WhenModelIsValid_ReturnsCreatedNote()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel(title: "Shopping list");

        // Act
        Result<Note> result = await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Message.Should().Be("Note created successfully");
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Shopping list");
    }

    [Fact]
    public async Task CreateAsync_WhenModelIsValid_StagesNoteOnRepository()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.NoteRepository.Verify(
            r => r.CreateAsync(result.Data!, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenModelIsValid_WritesAuditLogWithDomainResultMessage()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(
                It.Is<AuditLog>(a => a.EntityId == result.Data!.Id && a.Description == "Note created successfully"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenModelIsValid_InvalidatesCollectionsOnly()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Once);
        setup.CacheInvalidationService.Verify(c => c.InvalidateEntityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsEmpty_ReturnsUnprocessableEntity()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel(title: string.Empty);

        // Act
        Result<Note> result = await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsEmpty_DoesNotTouchRepositoriesOrCache()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        NoteCreateModel createModel = NoteTestData.CreateModel(title: string.Empty);

        // Act
        await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.NoteRepository.Verify(
            r => r.CreateAsync(It.IsAny<Note>(), It.IsAny<CancellationToken>()),
            Times.Never);
        setup.AuditLogRepository.Verify(
            r => r.CreateAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
        setup.CacheInvalidationService.Verify(c => c.InvalidateCollectionsAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenAuditLogStagingFails_StillReturnsCreatedNote()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.GivenAuditLogStagingFails();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        Result<Note> result = await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAsync_WhenAuditLogStagingFails_LogsWarning()
    {
        // Arrange
        NoteServiceTestSetup setup = new();
        setup.GivenAuditLogStagingFails();
        NoteCreateModel createModel = NoteTestData.CreateModel();

        // Act
        await setup.Service.CreateAsync(createModel, TestContext.Current.CancellationToken);

        // Assert
        setup.Logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
