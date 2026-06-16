using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Users.Login;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Security;
using Sentinal.Domain.Users;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace Sentinal.Tests;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILogger<LoginUserCommandHandler>> _mockLogger;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<LoginUserCommandHandler>>();

        _handler = new LoginUserCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object,
            _mockJwtTokenService.Object);
    }

    [Fact]
    public async Task Handle_WithValidEmailAndPassword_ReturnsToken()
    {
        // Arrange
        var command = new LoginUserCommand("password123", null, "test@example.com");
        var testUser = TestDataBuilder.CreateTestUser(email: "test@example.com");

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync("test@example.com"))
            .ReturnsAsync(testUser);
        _mockPasswordHasher.Setup(x => x.VerifyPassword("password123", testUser.PasswordHash))
            .Returns(true);
        _mockJwtTokenService.Setup(x => x.GenerateToken(testUser))
            .Returns("test-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(testUser.Id);
        result.Value.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Handle_WithValidUsernameAndPassword_ReturnsToken()
    {
        // Arrange
        var command = new LoginUserCommand("password123", "testuser", null);
        var testUser = TestDataBuilder.CreateTestUser(username: "testuser");

        _mockUserRepository.Setup(x => x.GetUserByUsernameAsync("testuser"))
            .ReturnsAsync(testUser);
        _mockPasswordHasher.Setup(x => x.VerifyPassword("password123", testUser.PasswordHash))
            .Returns(true);
        _mockJwtTokenService.Setup(x => x.GenerateToken(testUser))
            .Returns("test-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-token", result.Value.Token);
    }

    [Fact]
    public async Task Handle_WithNoEmailAndUsername_ReturnsFail()
    {
        // Arrange
        var command = new LoginUserCommand("password123", null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Email or Username is required"));
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_ReturnsFail()
    {
        // Arrange
        var command = new LoginUserCommand("", "testuser", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Password is required"));
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFail()
    {
        // Arrange
        var command = new LoginUserCommand("password123", "nonexistent", null);

        _mockUserRepository.Setup(x => x.GetUserByUsernameAsync("nonexistent"))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User not found"));
    }

    [Fact]
    public async Task Handle_WithDeletedUser_ReturnsFail()
    {
        // Arrange
        var command = new LoginUserCommand("password123", "deleted", null);
        var testUser = TestDataBuilder.CreateTestUser(username: "deleted");
        testUser.MarkedForDeletion = true;

        _mockUserRepository.Setup(x => x.GetUserByUsernameAsync("deleted"))
            .ReturnsAsync(testUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User not found"));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsFail()
    {
        // Arrange
        var command = new LoginUserCommand("wrongpassword", "testuser", null);
        var testUser = TestDataBuilder.CreateTestUser(username: "testuser");

        _mockUserRepository.Setup(x => x.GetUserByUsernameAsync("testuser"))
            .ReturnsAsync(testUser);
        _mockPasswordHasher.Setup(x => x.VerifyPassword("wrongpassword", testUser.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Invalid login attempt"));
    }
}
