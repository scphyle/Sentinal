using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.Delete;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class DeleteFolderCommandHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<DeleteFolderCommandHandler>> _mockLogger;
    private readonly DeleteFolderCommandHandler _handler;

    public DeleteFolderCommandHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<DeleteFolderCommandHandler>>();

        _handler = new DeleteFolderCommandHandler(
            _mockLogger.Object,
            _mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_SoftDeletesFolder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "ToDelete",
            userId: userId,
            id: folderId,
            markedForDeletion: false);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);
        _mockFolderRepository.Setup(x => x.MarkFolderAsDeletedAsync(folderId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(folderId);
        result.Value.FolderName.Should().Be("ToDelete");

        _mockFolderRepository.Verify(
            x => x.MarkFolderAsDeletedAsync(folderId, userId),
            Times.Once);
        _mockFolderRepository.Verify(
            x => x.DeleteFolderPermanentlyAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyFolderId_ReturnsFail()
    {
        // Arrange
        var command = new DeleteFolderCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder id cannot be empty"));

        _mockFolderRepository.Verify(x => x.GetFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new DeleteFolderCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User id cannot be empty"));

        _mockFolderRepository.Verify(x => x.GetFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentFolder_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync((Domain.Folders.FolderEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder not found or does not belong to you"));

        _mockFolderRepository.Verify(x => x.MarkFolderAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMarkedFolderSecondDelete_PermanentlyDeletesFolder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "ToDelete",
            userId: userId,
            id: folderId,
            markedForDeletion: true);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);
        _mockFolderRepository.Setup(x => x.DeleteFolderPermanentlyAsync(folderId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(folderId);
        result.Value.FolderName.Should().Be("ToDelete");

        _mockFolderRepository.Verify(
            x => x.DeleteFolderPermanentlyAsync(folderId),
            Times.Once);
        _mockFolderRepository.Verify(
            x => x.MarkFolderAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMarkedFolderButPermanentDeleteFails_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "ToDelete",
            userId: userId,
            id: folderId,
            markedForDeletion: true);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);
        _mockFolderRepository.Setup(x => x.DeleteFolderPermanentlyAsync(folderId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to permanently delete folder"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(userId: userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);
        _mockFolderRepository.Setup(x => x.MarkFolderAsDeletedAsync(folderId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to delete folder"));
    }

    [Fact]
    public async Task Handle_VerifyFolderDataReturnedAfterDeletion()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var folderName = "OriginalFolderName";
        var command = new DeleteFolderCommand(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: folderName,
            userId: userId,
            id: folderId,
            markedForDeletion: false);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);
        _mockFolderRepository.Setup(x => x.MarkFolderAsDeletedAsync(folderId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(folderId);
        result.Value.FolderName.Should().Be(folderName);
    }

    [Fact]
    public async Task Handle_WhenDeletingRecycleBin_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var recycleBinFolder = TestDataBuilder.CreateTestFolder(
            name: "RecycleBin",
            userId: userId,
            id: folderId,
            folderType: Sentinal.Domain.Folders.SpecialFolderTypes.RecycleBin);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(recycleBinFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Cannot delete special system folders"));
        _mockFolderRepository.Verify(x => x.MarkFolderAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDeletingHistory_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);
        var historyFolder = TestDataBuilder.CreateTestFolder(
            name: "History",
            userId: userId,
            id: folderId,
            folderType: Sentinal.Domain.Folders.SpecialFolderTypes.History);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(historyFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Cannot delete special system folders"));
        _mockFolderRepository.Verify(x => x.MarkFolderAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TwiceWithoutRemarking_FirstSoftDeletesThenPermanentlyDeletes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new DeleteFolderCommand(folderId, userId);

        // First call - folder not marked
        var folder = TestDataBuilder.CreateTestFolder(userId: userId, id: folderId, markedForDeletion: false);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(folder);
        _mockFolderRepository.Setup(x => x.MarkFolderAsDeletedAsync(folderId, userId))
            .ReturnsAsync(true);

        // Act - First delete
        var firstResult = await _handler.Handle(command, CancellationToken.None);

        // Assert first delete
        firstResult.IsSuccess.Should().BeTrue();
        _mockFolderRepository.Verify(
            x => x.MarkFolderAsDeletedAsync(folderId, userId),
            Times.Once);

        // Arrange - Second call - folder now marked
        var markedFolder = TestDataBuilder.CreateTestFolder(userId: userId, id: folderId, markedForDeletion: true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(markedFolder);
        _mockFolderRepository.Setup(x => x.DeleteFolderPermanentlyAsync(folderId))
            .ReturnsAsync(true);

        // Act - Second delete
        var secondResult = await _handler.Handle(command, CancellationToken.None);

        // Assert second delete
        secondResult.IsSuccess.Should().BeTrue();
        _mockFolderRepository.Verify(
            x => x.DeleteFolderPermanentlyAsync(folderId),
            Times.Once);
    }
}
