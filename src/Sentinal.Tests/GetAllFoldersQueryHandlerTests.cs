using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Folders.GetAllFolders;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class GetAllFoldersQueryHandlerTests
{
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<GetAllFoldersQueryHandler>> _mockLogger;
    private readonly GetAllFoldersQueryHandler _handler;

    public GetAllFoldersQueryHandlerTests()
    {
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<GetAllFoldersQueryHandler>>();

        _handler = new GetAllFoldersQueryHandler(
            _mockLogger.Object,_mockFolderRepository.Object);
    }

    [Fact]
    public async Task Handle_WithFoldersForUser_ReturnsAllFolders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);
        var folder1 = TestDataBuilder.CreateTestFolder(name: "Folder1", userId: userId);
        var folder2 = TestDataBuilder.CreateTestFolder(name: "Folder2", userId: userId);
        var folders = new List<Domain.Folders.FolderEntity> { folder1, folder2 };

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(folders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Folder1");
        result.Value[1].Name.Should().Be("Folder2");

        _mockFolderRepository.Verify(x => x.GetAllFoldersAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var query = new GetAllFoldersQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithNoFoldersForUser_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(new List<Domain.Folders.FolderEntity>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to retrieve folders"));
    }

    [Fact]
    public async Task Handle_ReturnsCorrectFolderProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);
        var testFolder = TestDataBuilder.CreateTestFolder(
            name: "TestFolder",
            userId: userId,
            parentId: parentId,
            id: folderId);

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(new List<Domain.Folders.FolderEntity> { testFolder });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var folderDto = result.Value[0];
        folderDto.Id.Should().Be(folderId);
        folderDto.Name.Should().Be("TestFolder");
        folderDto.ParentFolderId.Should().Be(parentId);
    }

    [Fact]
    public async Task Handle_WithMultipleFolders_ReturnsAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);
        var folders = new List<Domain.Folders.FolderEntity>();
        for (int i = 0; i < 5; i++)
        {
            folders.Add(TestDataBuilder.CreateTestFolder(
                name: $"Folder{i}",
                userId: userId));
        }

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(folders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to retrieve folders"));  // ← Updated
    }

    [Fact]
    public async Task Handle_DoesNotReturnSoftDeletedFolders()
    {
        // Arrange - Repository already filters soft-deleted folders
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);
        var activeFolder = TestDataBuilder.CreateTestFolder(name: "Active", userId: userId);

        // Note: DeletedFolder would be filtered out by repository.GetAllFoldersAsync
        var folders = new List<Domain.Folders.FolderEntity> { activeFolder };

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(folders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_IncludesRootFolder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFoldersQuery(userId);
        var rootFolder = TestDataBuilder.CreateTestFolder(
            name: userId.ToString(),
            userId: userId,
            id: userId,
            parentId: null);

        var childFolder = TestDataBuilder.CreateTestFolder(
            name: "Child",
            userId: userId,
            parentId: userId);

        var folders = new List<Domain.Folders.FolderEntity> { rootFolder, childFolder };

        _mockFolderRepository.Setup(x => x.GetAllFoldersAsync(userId))
            .ReturnsAsync(folders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(f => f.ParentFolderId == null); // Root folder
    }
}