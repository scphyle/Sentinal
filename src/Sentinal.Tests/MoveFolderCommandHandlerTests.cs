using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.Move;
using Sentinal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class MoveFolderCommandHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<MoveFolderCommandHandler>> _mockLogger;
    private readonly MoveFolderCommandHandler _handler;

    public MoveFolderCommandHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<MoveFolderCommandHandler>>();

        _handler = new MoveFolderCommandHandler(
            _mockLogger.Object,
            _mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_MovesFolder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceFolderId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFolderCommand(sourceFolderId, destinationFolderId, userId);

        var movedFolder = new Sentinal.Domain.Folders.FolderEntity
        {
            Id = sourceFolderId,
            FolderName = "TestFolder",
            ParentFolderId = destinationFolderId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFolderRepository.Setup(x => x.MoveFolderAsync(sourceFolderId, destinationFolderId, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(sourceFolderId, userId))
            .ReturnsAsync(movedFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentFolderId.Should().Be(destinationFolderId);
        result.Value.Id.Should().Be(sourceFolderId);

        _mockFolderRepository.Verify(
            x => x.MoveFolderAsync(sourceFolderId, destinationFolderId, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptySourceFolderId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFolderCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Source folder id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyDestinationFolderId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFolderCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Destination folder id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new MoveFolderCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithSourceAndDestinationSame_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new MoveFolderCommand(folderId, folderId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Source and destination folders cannot be the same"));

        _mockFolderRepository.Verify(x => x.MoveFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsFalse_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceFolderId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFolderCommand(sourceFolderId, destinationFolderId, userId);

        _mockFolderRepository.Setup(x => x.MoveFolderAsync(sourceFolderId, destinationFolderId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to move folder"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceFolderId = Guid.NewGuid();
        var destinationFolderId = Guid.NewGuid();
        var command = new MoveFolderCommand(sourceFolderId, destinationFolderId, userId);

        _mockFolderRepository.Setup(x => x.MoveFolderAsync(sourceFolderId, destinationFolderId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to move folder"));
    }

    [Fact]
    public async Task Handle_MovesFolderToDifferentParent_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sourceFolderId = Guid.NewGuid();
        var currentParentId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();
        var command = new MoveFolderCommand(sourceFolderId, newParentId, userId);

        var movedFolder = new Sentinal.Domain.Folders.FolderEntity
        {
            Id = sourceFolderId,
            FolderName = "TestFolder",
            ParentFolderId = newParentId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFolderRepository.Setup(x => x.MoveFolderAsync(sourceFolderId, newParentId, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(sourceFolderId, userId))
            .ReturnsAsync(movedFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentFolderId.Should().Be(newParentId);
    }

    [Fact]
    public async Task Handle_WithMultipleFolderMoves_EachCallInvokesRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folder1 = Guid.NewGuid();
        var folder2 = Guid.NewGuid();
        var destinationFolder = Guid.NewGuid();

        var command1 = new MoveFolderCommand(folder1, destinationFolder, userId);
        var command2 = new MoveFolderCommand(folder2, destinationFolder, userId);

        var movedFolder1 = new Sentinal.Domain.Folders.FolderEntity
        {
            Id = folder1,
            FolderName = "Folder1",
            ParentFolderId = destinationFolder,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var movedFolder2 = new Sentinal.Domain.Folders.FolderEntity
        {
            Id = folder2,
            FolderName = "Folder2",
            ParentFolderId = destinationFolder,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFolderRepository.Setup(x => x.MoveFolderAsync(folder1, destinationFolder, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.MoveFolderAsync(folder2, destinationFolder, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folder1, userId))
            .ReturnsAsync(movedFolder1);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folder2, userId))
            .ReturnsAsync(movedFolder2);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        _mockFolderRepository.Verify(x => x.MoveFolderAsync(folder1, destinationFolder, userId), Times.Once);
        _mockFolderRepository.Verify(x => x.MoveFolderAsync(folder2, destinationFolder, userId), Times.Once);
    }
}