using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.Create;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class CreateFolderCommandHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<CreateFolderCommandHandler>> _mockLogger;
    private readonly CreateFolderCommandHandler _handler;

    public CreateFolderCommandHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<CreateFolderCommandHandler>>();

        _handler = new CreateFolderCommandHandler(
            _mockLogger.Object,
            _mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidDataAndNoParent_CreatesFolderWithRootAsParent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateFolderCommand("TestFolder", userId, null);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "TestFolder",
            userId: userId,
            parentId: userId);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync("TestFolder", userId, userId))
            .ReturnsAsync(false);
        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsAsync(userId, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.CreateFolderAsync("TestFolder", userId, userId, null))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FolderId.Should().Be(testFolder.Id);
        result.Value.Name.Should().Be("TestFolder");
        result.Value.ParentId.Should().Be(userId);

        _mockFolderRepository.Verify(
            x => x.CreateFolderAsync("TestFolder", userId, userId, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidDataAndParent_CreatesFolderUnderParent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var command = new CreateFolderCommand("TestFolder", userId, parentId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "TestFolder",
            userId: userId,
            parentId: parentId);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync("TestFolder", parentId, userId))
            .ReturnsAsync(false);
        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsAsync(parentId, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.CreateFolderAsync("TestFolder", userId, parentId, null))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FolderId.Should().Be(testFolder.Id);
        result.Value.ParentId.Should().Be(parentId);

        _mockFolderRepository.Verify(
            x => x.CreateFolderAsync("TestFolder", userId, parentId, null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFolderName_ReturnsFail()
    {
        // Arrange
        var command = new CreateFolderCommand("", Guid.NewGuid(), null);

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
        var command = new CreateFolderCommand("   ", Guid.NewGuid(), null);

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
        var command = new CreateFolderCommand(longName, Guid.NewGuid(), null);

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
        var command = new CreateFolderCommand("TestFolder", Guid.Empty, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyParentId_ReturnsFail()
    {
        // Arrange
        var command = new CreateFolderCommand("TestFolder", Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Parent folder cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithDuplicateFolderName_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateFolderCommand("TestFolder", userId, null);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync("TestFolder", userId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("A folder with this name already exists under the parent directory"));

        _mockFolderRepository.Verify(x => x.CreateFolderAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentParentFolder_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var command = new CreateFolderCommand("TestFolder", userId, parentId);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync("TestFolder", parentId, userId))
            .ReturnsAsync(false);
        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsAsync(parentId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Parent folder does not exist"));

        _mockFolderRepository.Verify(x => x.CreateFolderAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateFolderCommand("TestFolder", userId, null);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);
        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.CreateFolderAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to create folder"));
    }

    [Fact]
    public async Task Handle_MaxLengthFolderName_CreatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var maxLengthName = new string('a', 255);
        var command = new CreateFolderCommand(maxLengthName, userId, null);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: maxLengthName,
            userId: userId,
            parentId: userId);

        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsUnderParentAsync(maxLengthName, userId, userId))
            .ReturnsAsync(false);
        _mockFolderRepository.Setup(x => x.CheckIfFolderExistsAsync(userId, userId))
            .ReturnsAsync(true);
        _mockFolderRepository.Setup(x => x.CreateFolderAsync(maxLengthName, userId, userId, null))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
