using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Files.Delete;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class DeleteFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<ILogger<DeleteFileCommandHandler>> _mockLogger;
    private readonly DeleteFileCommandHandler _handler;

    public DeleteFileCommandHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockLogger = new Mock<ILogger<DeleteFileCommandHandler>>();

        _handler = new DeleteFileCommandHandler(
            _mockFileRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_SoftDeletesFile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);
        var file = TestDataBuilder.CreateTestFile(id: fileId, userId: userId, markedForDeletion: false);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(file);
        _mockFileRepository.Setup(x => x.MarkFileAsDeletedAsync(fileId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockFileRepository.Verify(
            x => x.MarkFileAsDeletedAsync(fileId, userId),
            Times.Once);
        _mockFileRepository.Verify(
            x => x.PermanentlyDeleteFileAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyFileId_ReturnsFail()
    {
        // Arrange
        var command = new DeleteFileCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("FileId cannot be empty"));

        _mockFileRepository.Verify(x => x.GetFileAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new DeleteFileCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("UserId cannot be empty"));

        _mockFileRepository.Verify(x => x.GetFileAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync((Domain.Files.FileEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("File not found or user does not own this file"));
    }

    [Fact]
    public async Task Handle_WithMarkedFileSecondDelete_PermanentlyDeletesFile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);
        var file = TestDataBuilder.CreateTestFile(id: fileId, userId: userId, markedForDeletion: true);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(file);
        _mockFileRepository.Setup(x => x.PermanentlyDeleteFileAsync(fileId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockFileRepository.Verify(
            x => x.PermanentlyDeleteFileAsync(fileId, userId),
            Times.Once);
        _mockFileRepository.Verify(
            x => x.MarkFileAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMarkedFileButPermanentDeleteFails_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);
        var file = TestDataBuilder.CreateTestFile(id: fileId, userId: userId, markedForDeletion: true);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(file);
        _mockFileRepository.Setup(x => x.PermanentlyDeleteFileAsync(fileId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to permanently delete file"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Error deleting file"));
    }

    [Fact]
    public async Task Handle_TwiceWithoutRemarking_FirstSoftDeletesThenPermanentlyDeletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new DeleteFileCommand(fileId, userId);

        // First call - file not marked
        var file = TestDataBuilder.CreateTestFile(id: fileId, userId: userId, markedForDeletion: false);
        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(file);
        _mockFileRepository.Setup(x => x.MarkFileAsDeletedAsync(fileId, userId))
            .ReturnsAsync(true);

        // Act - First delete
        var firstResult = await _handler.Handle(command, CancellationToken.None);

        // Assert first delete
        firstResult.IsSuccess.Should().BeTrue();
        _mockFileRepository.Verify(
            x => x.MarkFileAsDeletedAsync(fileId, userId),
            Times.Once);

        // Arrange - Second call - file now marked
        var markedFile = TestDataBuilder.CreateTestFile(id: fileId, userId: userId, markedForDeletion: true);
        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(markedFile);
        _mockFileRepository.Setup(x => x.PermanentlyDeleteFileAsync(fileId, userId))
            .ReturnsAsync(true);

        // Act - Second delete
        var secondResult = await _handler.Handle(command, CancellationToken.None);

        // Assert second delete
        secondResult.IsSuccess.Should().BeTrue();
        _mockFileRepository.Verify(
            x => x.PermanentlyDeleteFileAsync(fileId, userId),
            Times.Once);
    }
}
