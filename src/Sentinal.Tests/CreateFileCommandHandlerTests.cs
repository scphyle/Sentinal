using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Sentinal.Application.Files.Create;

namespace Sentinal.Tests;

public class CreateFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ILogger<CreateFileCommandHandler>> _mockLogger;
    private readonly CreateFileCommandHandler _handler;

    public CreateFileCommandHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<CreateFileCommandHandler>>();

        _handler = new CreateFileCommandHandler(
            _mockFileStorageService.Object,
            _mockFileRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_CreatesFileAndSavesContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            userId,
            folderId,
            "Test file");

        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "test.txt",
            userId: userId,
            folderId: folderId,
            fileSize: fileStream.Length);

        _mockFileRepository.Setup(x => x.CreateFileAsync(
            "test.txt",
            fileStream.Length,
            "text/plain",
            userId,
            folderId,
            "Test file"))
            .ReturnsAsync(testFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(testFile.Id);

        _mockFileRepository.Verify(
            x => x.CreateFileAsync(
                "test.txt",
                fileStream.Length,
                "text/plain",
                userId,
                folderId,
                "Test file"),
            Times.Once);

        _mockFileStorageService.Verify(
            x => x.SaveFileAsync(userId, testFile.Id, fileStream),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFolderId_ReturnsFail()
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            Guid.NewGuid(),
            Guid.Empty,
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("FolderId cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            Guid.Empty,
            Guid.NewGuid(),
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("UserId cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyFileName_ReturnsFail()
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new CreateFileCommand(
            "",
            "text/plain",
            fileStream,
            fileStream.Length,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyContentType_ReturnsFail()
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new CreateFileCommand(
            "test.txt",
            "",
            fileStream,
            fileStream.Length,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("ContentType cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithWhitespaceFileName_ReturnsFail()
    {
        // Arrange
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new CreateFileCommand(
            "   ",
            "text/plain",
            fileStream,
            fileStream.Length,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_CleanupFileAndReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            userId,
            folderId,
            null);

        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "test.txt",
            userId: userId,
            folderId: folderId);

        _mockFileRepository.Setup(x => x.CreateFileAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(testFile);

        _mockFileStorageService.Setup(x => x.SaveFileAsync(userId, testFile.Id, fileStream))
            .ThrowsAsync(new Exception("Storage service error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Error creating file"));

        // Verify cleanup was called
        _mockFileRepository.Verify(
            x => x.MarkFileAsDeletedAsync(testFile.Id, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFileCreationFails_DoesNotAttemptCleanup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            userId,
            folderId,
            null);

        _mockFileRepository.Setup(x => x.CreateFileAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Error creating file"));

        // Verify cleanup was NOT called since file was never created
        _mockFileRepository.Verify(
            x => x.MarkFileAsDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithOptionalDescription_CreatesFileWithDescription()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var description = "This is a test file";
        var command = new CreateFileCommand(
            "test.txt",
            "text/plain",
            fileStream,
            fileStream.Length,
            userId,
            folderId,
            description);

        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "test.txt",
            userId: userId,
            folderId: folderId,
            description: description);

        _mockFileRepository.Setup(x => x.CreateFileAsync(
            "test.txt",
            fileStream.Length,
            "text/plain",
            userId,
            folderId,
            description))
            .ReturnsAsync(testFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockFileRepository.Verify(
            x => x.CreateFileAsync(
                "test.txt",
                fileStream.Length,
                "text/plain",
                userId,
                folderId,
                description),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentContentTypes_CreatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var contentType = "application/pdf";
        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var command = new CreateFileCommand(
            "document.pdf",
            contentType,
            fileStream,
            fileStream.Length,
            userId,
            folderId,
            null);

        var testFile = TestDataBuilder.CreateTestFile(
            fileName: "document.pdf",
            userId: userId,
            folderId: folderId,
            contentType: contentType);

        _mockFileRepository.Setup(x => x.CreateFileAsync(
            "document.pdf",
            fileStream.Length,
            contentType,
            userId,
            folderId,
            null))
            .ReturnsAsync(testFile);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
