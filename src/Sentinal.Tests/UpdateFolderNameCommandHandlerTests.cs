using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.Update;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class UpdateFolderNameCommandHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<UpdateFolderNameCommandHandler>> _mockLogger;
    private readonly UpdateFolderNameCommandHandler _handler;

    public UpdateFolderNameCommandHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<UpdateFolderNameCommandHandler>>();

        _handler = new UpdateFolderNameCommandHandler(
            _mockLogger.Object,
            _mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesFolderName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var newName = "UpdatedFolderName";
        var command = new UpdateFolderNameCommand(folderId, newName, userId);

        var updatedFolder = TestDataBuilder.CreateTestFolder(id: folderId, name: newName, userId: userId);

        _mockFolderRepository.Setup(x => x.UpdateFolderNameAsync(folderId, newName, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(updatedFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(newName);
        result.Value.Id.Should().Be(folderId);

        _mockFolderRepository.Verify(
            x => x.UpdateFolderNameAsync(folderId, newName, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFolderId_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFolderNameCommand(Guid.Empty, "NewName", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyFolderName_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFolderNameCommand(Guid.NewGuid(), "", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithWhitespaceFolderName_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFolderNameCommand(Guid.NewGuid(), "   ", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithLongFolderName_ReturnsFail()
    {
        // Arrange
        var longName = new string('a', 256);
        var command = new UpdateFolderNameCommand(Guid.NewGuid(), longName, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder name cannot be longer than 255 characters"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFolderNameCommand(Guid.NewGuid(), "NewName", Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsFalse_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new UpdateFolderNameCommand(folderId, "NewName", userId);

        _mockFolderRepository.Setup(x => x.UpdateFolderNameAsync(folderId, "NewName", userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to update folder name"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var command = new UpdateFolderNameCommand(folderId, "NewName", userId);

        _mockFolderRepository.Setup(x => x.UpdateFolderNameAsync(folderId, "NewName", userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to update folder name"));
    }

    [Fact]
    public async Task Handle_WithMaxLengthFolderName_UpdatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var maxLengthName = new string('a', 255);
        var command = new UpdateFolderNameCommand(folderId, maxLengthName, userId);

        var updatedFolder = TestDataBuilder.CreateTestFolder(id: folderId, name: maxLengthName, userId: userId);

        _mockFolderRepository.Setup(x => x.UpdateFolderNameAsync(folderId, maxLengthName, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(updatedFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(maxLengthName);
    }
}
