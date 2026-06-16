using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Files.Move;

namespace Sentinal.Tests;

public class MoveFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<ILogger<MoveFileCommandHandler>> _mockLogger;
    private readonly MoveFileCommandHandler _handler;

    public MoveFileCommandHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockLogger = new Mock<ILogger<MoveFileCommandHandler>>();

        _handler = new MoveFileCommandHandler(
            _mockLogger.Object,
            _mockFileRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_MovesFile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFileCommand(fileId, destinationFolderId, userId);

        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId, destinationFolderId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockFileRepository.Verify(
            x => x.MoveFileAsync(fileId, destinationFolderId, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFileId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFileCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("File ID cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFileCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User ID cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyDestinationFolderId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFileCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Destination folder cannot be empty"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsFalse_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFileCommand(fileId, destinationFolderId, userId);

        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId, destinationFolderId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to move file"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFileCommand(fileId, destinationFolderId, userId);

        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId, destinationFolderId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to move file"));
    }

    [Fact]
    public async Task Handle_MovesFileToDifferentFolder_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var sourceFolder = Guid.NewGuid();
        var destinationFolder = Guid.NewGuid();
        var command = new MoveFileCommand(fileId, destinationFolder, userId);

        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId, destinationFolder, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockFileRepository.Verify(x => x.MoveFileAsync(fileId, destinationFolder, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleFileMoves_EachCallInvokesRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId1 = Guid.NewGuid();
        var fileId2 = Guid.NewGuid();
        var destinationFolder = Guid.NewGuid();

        var command1 = new MoveFileCommand(fileId1, destinationFolder, userId);
        var command2 = new MoveFileCommand(fileId2, destinationFolder, userId);

        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId1, destinationFolder, userId))
            .ReturnsAsync(true);
        _mockFileRepository.Setup(x => x.MoveFileAsync(fileId2, destinationFolder, userId))
            .ReturnsAsync(true);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        _mockFileRepository.Verify(x => x.MoveFileAsync(fileId1, destinationFolder, userId), Times.Once);
        _mockFileRepository.Verify(x => x.MoveFileAsync(fileId2, destinationFolder, userId), Times.Once);
    }
}
