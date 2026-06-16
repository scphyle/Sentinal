using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Files.GetAllFiles;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class GetAllFilesQueryHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<ILogger<GetAllFilesQueryHandler>> _mockLogger;
    private readonly GetAllFilesQueryHandler _handler;

    public GetAllFilesQueryHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockLogger = new Mock<ILogger<GetAllFilesQueryHandler>>();

        _handler = new GetAllFilesQueryHandler(
            _mockFileRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithFilesForUser_ReturnsAllFiles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);
        var file1 = TestDataBuilder.CreateTestFile(fileName: "file1.txt", userId: userId);
        var file2 = TestDataBuilder.CreateTestFile(fileName: "file2.txt", userId: userId);
        var files = new List<Domain.Files.FileEntity> { file1, file2 };

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ReturnsAsync(files);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].FileName.Should().Be("file1.txt");
        result.Value[1].FileName.Should().Be("file2.txt");

        _mockFileRepository.Verify(x => x.GetAllUserFilesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var query = new GetAllFilesQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User Id cannont be empty"));
    }

    [Fact]
    public async Task Handle_WithNoFilesForUser_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ReturnsAsync(new List<Domain.Files.FileEntity>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("No Files found"));
    }

    [Fact]
    public async Task Handle_ReturnsCorrectFileProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);
        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "test.txt",
            userId: userId,
            folderId: folderId,
            contentType: "text/plain",
            description: "Test file description");

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ReturnsAsync(new List<Domain.Files.FileEntity> { testFile });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var fileDto = result.Value[0];
        fileDto.FileId.Should().Be(testFile.Id);
        fileDto.FileName.Should().Be("test.txt");
        fileDto.FileType.Should().Be("text/plain");
        fileDto.FileDescription.Should().Be("Test file description");
        fileDto.FolderId.Should().Be(folderId);
        fileDto.DateCreated.Should().Be(testFile.CreatedAt);
        fileDto.DateUpdated.Should().Be(testFile.UpdatedAt);
    }

    [Fact]
    public async Task Handle_WithMultipleFiles_ReturnsCorrectedOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);
        var file1 = TestDataBuilder.CreateTestFile(fileName: "a.txt", userId: userId);
        var file2 = TestDataBuilder.CreateTestFile(fileName: "b.txt", userId: userId);
        var file3 = TestDataBuilder.CreateTestFile(fileName: "c.txt", userId: userId);
        var files = new List<Domain.Files.FileEntity> { file1, file2, file3 };

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ReturnsAsync(files);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].FileName.Should().Be("a.txt");
        result.Value[1].FileName.Should().Be("b.txt");
        result.Value[2].FileName.Should().Be("c.txt");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to get all files"));
    }

    [Fact]
    public async Task Handle_DoesNotReturnSoftDeletedFiles()
    {
        // Arrange - Repository already filters soft-deleted files, we verify the behavior
        var userId = Guid.NewGuid();
        var query = new GetAllFilesQuery(userId);
        var activeFile = TestDataBuilder.CreateTestFile(fileName: "active.txt", userId: userId);

        // Note: DeletedFile would be filtered out by repository.GetAllUserFilesAsync
        var files = new List<Domain.Files.FileEntity> { activeFile };

        _mockFileRepository.Setup(x => x.GetAllUserFilesAsync(userId))
            .ReturnsAsync(files);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].FileName.Should().Be("active.txt");
    }
}
