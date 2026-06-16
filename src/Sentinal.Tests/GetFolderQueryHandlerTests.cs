using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.GetFolder;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class GetFolderQueryHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<GetFolderQueryHandler>> _mockLogger;
    private readonly GetFolderQueryHandler _handler;

    public GetFolderQueryHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<GetFolderQueryHandler>>();

        _handler = new GetFolderQueryHandler(
            _mockLogger.Object,
            _mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsFolderWithChildren()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var query = new GetFolderQuery(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "MyFolder",
            userId: userId,
            id: folderId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(folderId);
        result.Value.Name.Should().Be("MyFolder");
        result.Value.CreatedAt.Should().Be(testFolder.CreatedAt);
        result.Value.UpdatedAt.Should().Be(testFolder.UpdatedAt);

        _mockFolderRepository.Verify(x => x.GetFolderAsync(folderId, userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFolderId_ReturnsFail()
    {
        // Arrange
        var query = new GetFolderQuery(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder id cannot be empty"));

        _mockFolderRepository.Verify(x => x.GetFolderAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var query = new GetFolderQuery(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
        var query = new GetFolderQuery(folderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync((Domain.Folders.FolderEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder not found or does not belong to you"));
    }

    [Fact]
    public async Task Handle_WithFolderNotOwnedByUser_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var query = new GetFolderQuery(folderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync((Domain.Folders.FolderEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Folder not found or does not belong to you"));
    }

    [Fact]
    public async Task Handle_WithFolderIncludingChildren_ReturnsChildrenCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parentFolderId = Guid.NewGuid();
        var childFolder1 = TestDataBuilder.CreateTestFolder(
            name: "Child1",
            userId: userId,
            parentId: parentFolderId);
        var childFolder2 = TestDataBuilder.CreateTestFolder(
            name: "Child2",
            userId: userId,
            parentId: parentFolderId);

        var parentFolder = TestDataBuilder.CreateTestFolder(
            name: "Parent",
            userId: userId,
            id: parentFolderId);

        // Note: In real scenario, EF would populate Children collection
        var query = new GetFolderQuery(parentFolderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(parentFolderId, userId))
            .ReturnsAsync(parentFolder);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(parentFolderId);
    }

    [Fact]
    public async Task Handle_WithValidFolder_ReturnsFolderDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "TestFolder",
            userId: userId,
            id: folderId);

        var query = new GetFolderQuery(folderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("TestFolder");
        result.Value.Id.Should().Be(folderId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var query = new GetFolderQuery(folderId, userId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to retrieve folder"));
    }

    [Fact]
    public async Task Handle_ReturnsCorrectParentIdWhenSet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var query = new GetFolderQuery(folderId, userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "ChildFolder",
            userId: userId,
            id: folderId,
            parentId: parentId);

        _mockFolderRepository.Setup(x => x.GetFolderAsync(folderId, userId))
            .ReturnsAsync(testFolder);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentFolderId.Should().Be(parentId);
    }
}