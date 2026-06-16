using Xunit;
using Moq;
using FluentAssertions;
using Sentinal.Application.Users.Register;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Security;
using Sentinal.Domain.Users;
using Sentinal.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Sentinal.Domain.Folders;

namespace Sentinal.Tests;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<IFolderRepository> _mockFolderRepository;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _mockLogger;
    private readonly Mock<IRegistrationPolicy> _mockRegistrationPolicy;

    public RegisterUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockFolderRepository = new Mock<IFolderRepository>();
        _mockLogger = new Mock<ILogger<RegisterUserCommandHandler>>();
        _mockRegistrationPolicy = new Mock<IRegistrationPolicy>();
    }

    private RegisterUserCommandHandler Handler
    {
        get
        {
            _mockRegistrationPolicy.Setup(x => x.IsRegistrationEnabled())
                .Returns(true);

            return new RegisterUserCommandHandler(
                _mockUserRepository.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object,
                _mockJwtTokenService.Object,
                _mockFileStorageService.Object,
                _mockFolderRepository.Object,
                _mockRegistrationPolicy.Object);
        }
    }

    private RegisterUserCommandHandler CreateHandler(bool enableRegistration)
    {
        _mockRegistrationPolicy.Setup(x => x.IsRegistrationEnabled())
            .Returns(enableRegistration);

        return new RegisterUserCommandHandler(
            _mockUserRepository.Object,
            _mockPasswordHasher.Object,
            _mockLogger.Object,
            _mockJwtTokenService.Object,
            _mockFileStorageService.Object,
            _mockFolderRepository.Object,
            _mockRegistrationPolicy.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_RegistersUserSuccessfully()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@example.com", "password123");
        var testUser = TestDataBuilder.CreateTestUser(
            username: command.Username,
            email: command.Email);

        _mockPasswordHasher.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedpassword");
        _mockUserRepository.Setup(x => x.CreateUserAsync(command.Username, command.Email, It.IsAny<string>()))
            .ReturnsAsync(testUser);
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
            .Returns("test-token");
        _mockFolderRepository.Setup(x => x.CreateRootFolderAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(TestDataBuilder.CreateTestFolder());
        _mockFolderRepository.Setup(x => x.CreateFolderAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<SpecialFolderTypes>()))
            .ReturnsAsync(TestDataBuilder.CreateTestFolder());

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(testUser.Id);
        result.Value.Username.Should().Be(testUser.Username);
        result.Value.Email.Should().Be(testUser.Email);
        result.Value.Token.Should().Be("test-token");

        _mockUserRepository.Verify(
            x => x.CreateUserAsync(command.Username, command.Email, "hashedpassword"),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRegistrationDisabled_ReturnsFail()
    {
        // Arrange
        var handler = CreateHandler(enableRegistration: false);
        var command = new RegisterUserCommand("testuser", "test@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("User registration is currently disabled"));

        _mockUserRepository.Verify(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyUsername_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("", "test@example.com", "password123");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Username, password and email are required"));
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "", "password123");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Username, password and email are required"));
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@example.com", "");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Username, password and email are required"));
    }

    [Fact]
    public async Task Handle_WithShortUsername_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("ab", "test@example.com", "password123");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Username must be between 3 and 255 characters"));
    }

    [Fact]
    public async Task Handle_WithLongUsername_ReturnsFail()
    {
        // Arrange
        var longUsername = new string('a', 256);
        var command = new RegisterUserCommand(longUsername, "test@example.com", "password123");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Username must be between 3 and 255 characters"));
    }

    [Fact]
    public async Task Handle_WithShortEmail_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "a@b.c", "password123");

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Email must be between 7 and 255 characters"));
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@example.com", "password123");

        _mockPasswordHasher.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedpassword");
        _mockUserRepository.Setup(x => x.CreateUserAsync(command.Username, command.Email, It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("duplicate email"));

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Email already exists"));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFail()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@example.com", "password123");

        _mockPasswordHasher.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedpassword");
        _mockUserRepository.Setup(x => x.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Select(e => e.Message).Should().Contain(m => m.Contains("Failed to create user"));
    }
}
