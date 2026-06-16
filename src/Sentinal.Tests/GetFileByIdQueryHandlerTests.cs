using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Files.GetFile;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class GetFileByIdQueryHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ILogger<GetFileByIdQueryHandler>> _mockLogger;
    private readonly GetFileByIdQueryHandler _handler;

    public GetFileByIdQueryHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<GetFileByIdQueryHandler>>();

        _handler = new GetFileByIdQueryHandler(
            _mockLogger.Object,
            _mockFileRepository.Object,
            _mockFileStorageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsFileContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var query = new GetFileByIdQuery(fileId, userId);
        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "test.txt",
            userId: userId,
            id: fileId);

        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(testFile);
        _mockFileStorageService.Setup(x => x.GetFileAsync(userId, fileId))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.txt");
        result.Value.ContentType.Should().Be("text/plain");
        result.Value.FileId.Should().Be(fileId);
        result.Value.FileStream.Should().NotBeNull();

        _mockFileRepository.Verify(x => x.GetFileAsync(fileId, userId), Times.Once);
        _mockFileStorageService.Verify(x => x.GetFileAsync(userId, fileId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFileId_ReturnsFail()
    {
        // Arrange
        var query = new GetFileByIdQuery(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("File Id Required"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var query = new GetFileByIdQuery(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User Id Required"));
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var query = new GetFileByIdQuery(fileId, userId);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync((Domain.Files.FileEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("File Not Found"));

        _mockFileStorageService.Verify(x => x.GetFileAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStorageReturnsEmptyStream_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var query = new GetFileByIdQuery(fileId, userId);
        var testFile = TestDataBuilder.CreateTestFile(userId: userId, id: fileId);

        var emptyStream = new MemoryStream(new byte[] { });

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(testFile);
        _mockFileStorageService.Setup(x => x.GetFileAsync(userId, fileId))
            .ReturnsAsync(emptyStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("File Not Found"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var query = new GetFileByIdQuery(fileId, userId);

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Error getting file"));
    }

    [Fact]
    public async Task Handle_WithDifferentContentTypes_ReturnsCorrectType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var query = new GetFileByIdQuery(fileId, userId);
        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "document.pdf",
            userId: userId,
            id: fileId,
            contentType: "application/pdf");

        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockFileRepository.Setup(x => x.GetFileAsync(fileId, userId))
            .ReturnsAsync(testFile);
        _mockFileStorageService.Setup(x => x.GetFileAsync(userId, fileId))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
        result.Value.FileName.Should().Be("document.pdf");
    }
}
