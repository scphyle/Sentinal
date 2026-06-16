using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Files.UpdateFileName;
using Sentinal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class UpdateFileNameCommandHandlerTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<ILogger<UpdateFileNameCommandHandler>> _mockLogger;
    private readonly UpdateFileNameCommandHandler _handler;

    public UpdateFileNameCommandHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockLogger = new Mock<ILogger<UpdateFileNameCommandHandler>>();

        _handler = new UpdateFileNameCommandHandler(
            _mockLogger.Object,
            _mockFileRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_UpdatesFileName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var newFileName = "updated.txt";
        var command = new UpdateFileNameCommand(fileId, userId, newFileName);

        _mockFileRepository.Setup(x => x.UpdateFileNameAsync(fileId, newFileName, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockFileRepository.Verify(
            x => x.UpdateFileNameAsync(fileId, newFileName, userId),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyFileId_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFileNameCommand(Guid.Empty, Guid.NewGuid(), "newname.txt");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("FileId cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFileNameCommand(Guid.NewGuid(), Guid.Empty, "newname.txt");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User Id cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithEmptyFileName_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFileNameCommand(Guid.NewGuid(), Guid.NewGuid(), "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("New file name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WithNullFileName_ReturnsFail()
    {
        // Arrange
        var command = new UpdateFileNameCommand(Guid.NewGuid(), Guid.NewGuid(), null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("New file name cannot be empty"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsFalse_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new UpdateFileNameCommand(fileId, userId, "newname.txt");

        _mockFileRepository.Setup(x => x.UpdateFileNameAsync(fileId, "newname.txt", userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to update file name"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var command = new UpdateFileNameCommand(fileId, userId, "newname.txt");

        _mockFileRepository.Setup(x => x.UpdateFileNameAsync(fileId, "newname.txt", userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Error updating file name"));
    }

    [Fact]
    public async Task Handle_WithDifferentFileExtensions_UpdatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var newFileName = "document.pdf";
        var command = new UpdateFileNameCommand(fileId, userId, newFileName);

        _mockFileRepository.Setup(x => x.UpdateFileNameAsync(fileId, newFileName, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithLongFileName_UpdatesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var longFileName = new string('a', 250) + ".txt";
        var command = new UpdateFileNameCommand(fileId, userId, longFileName);

        _mockFileRepository.Setup(x => x.UpdateFileNameAsync(fileId, longFileName, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
