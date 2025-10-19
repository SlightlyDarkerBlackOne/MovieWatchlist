using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Tests.Infrastructure;
using System.Security.Claims;
using Xunit;

namespace MovieWatchlist.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService
/// </summary>
public class AuthenticationServiceTests : UnitTestBase
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordResetTokenRepository> _mockPasswordResetTokenRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockPasswordResetTokenRepository = new Mock<IPasswordResetTokenRepository>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmailService = new Mock<IEmailService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        // Setup password hasher to use real implementation for testing
        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns<string>(pwd => new MovieWatchlist.Infrastructure.Services.PasswordHasher().HashPassword(pwd));
        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((pwd, hash) => new MovieWatchlist.Infrastructure.Services.PasswordHasher().VerifyPassword(pwd, hash));

        _jwtSettings = new JwtSettings
        {
            SecretKey = "TestSecretKey123456789012345678901234567890",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };

        var options = Options.Create(_jwtSettings);
        _authenticationService = new AuthenticationService(
            _mockUserRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockPasswordResetTokenRepository.Object,
            _mockJwtTokenService.Object,
            _mockUnitOfWork.Object,
            options,
            _mockEmailService.Object,
            _mockPasswordHasher.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword
        );

        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null); // No existing users
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null); // No existing users
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        // Act
        var result = await _authenticationService.RegisterAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("test-jwt-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(TestConstants.Users.DefaultUsername);
        result.User.Email.Should().Be(TestConstants.Users.DefaultEmail);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: "testuser",
            Email: "existing@example.com",
            Password: "Password123!"
        );

        var existingUser = CreateTestUser(email: "existing@example.com");
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser); // Existing user with same email

        // Act
        var result = await _authenticationService.RegisterAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Email is already registered");
        result.Token.Should().BeNull();
        result.User.Should().BeNull();

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: "existinguser",
            Email: "test@example.com",
            Password: "Password123!"
        );

        // First call returns null (email check), second call returns existing user (username check)
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null); // No existing email
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.Username))
            .ReturnsAsync(CreateTestUser(username: "existinguser")); // Existing username

        // Act
        var result = await _authenticationService.RegisterAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username is already taken");
        result.Token.Should().BeNull();
        result.User.Should().BeNull();

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultUsername,
            Password: TestConstants.Users.DefaultPassword
        );

        var user = TestDataBuilder.User()
            .WithId(1)
            .WithUsername(TestConstants.Users.DefaultUsername)
            .WithEmail(TestConstants.Users.DefaultEmail)
            .WithPasswordHash(new PasswordHasher().HashPassword(TestConstants.Users.DefaultPassword))
            .Build();

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("test-jwt-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(TestConstants.Users.DefaultUsername);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailureResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: "nonexistent",
            Password: "Password123!"
        );

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.UsernameOrEmail))
            .ReturnsAsync((User?)null); // No user found
        _mockUserRepository.Setup(x => x.GetByEmailAsync(command.UsernameOrEmail))
            .ReturnsAsync((User?)null); // No user found

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username/email or password");
        result.Token.Should().BeNull();
        result.User.Should().BeNull();

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultUsername,
            Password: "WrongPassword123!"
        );

        var user = TestDataBuilder.User()
            .WithId(1)
            .WithUsername(TestConstants.Users.DefaultUsername)
            .WithEmail(TestConstants.Users.DefaultEmail)
            .WithPasswordHash(new PasswordHasher().HashPassword("CorrectPassword123!"))
            .Build();

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(command.UsernameOrEmail))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username/email or password");
        result.Token.Should().BeNull();
        result.User.Should().BeNull();

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmail_ReturnsSuccessResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword
        );

        var user = TestDataBuilder.User()
            .WithId(1)
            .WithUsername(TestConstants.Users.DefaultUsername)
            .WithEmail(TestConstants.Users.DefaultEmail)
            .WithPasswordHash(new PasswordHasher().HashPassword(TestConstants.Users.DefaultPassword))
            .Build();

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("test-jwt-token");
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(TestConstants.Users.DefaultEmail);
    }

    #endregion

    #region ValidateTokenAsync Tests

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var validToken = "valid-jwt-token";
        var mockPrincipal = new ClaimsPrincipal();
        
        _mockJwtTokenService.Setup(x => x.ValidateToken(validToken))
            .Returns(mockPrincipal);

        // Act
        var result = await _authenticationService.ValidateTokenAsync(validToken);

        // Assert
        result.Should().BeTrue();
        _mockJwtTokenService.Verify(x => x.ValidateToken(validToken), Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid-jwt-token";
        
        _mockJwtTokenService.Setup(x => x.ValidateToken(invalidToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var result = await _authenticationService.ValidateTokenAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
        _mockJwtTokenService.Verify(x => x.ValidateToken(invalidToken), Times.Once);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsNewToken()
    {
        // Arrange
        var validRefreshToken = "valid-refresh-token";
        var user = CreateTestUser(id: 1, username: TestConstants.Users.DefaultUsername, email: TestConstants.Users.DefaultEmail);
        var refreshTokenEntity = TestDataBuilder.RefreshToken()
            .WithId(1)
            .WithUserId(1)
            .WithToken(validRefreshToken)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .WithIsRevoked(false)
            .Build();

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(refreshTokenEntity);
        _mockUserRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(user))
            .Returns("new-jwt-token");
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        var result = await _authenticationService.RefreshTokenAsync(validRefreshToken);

        // Assert
        result.Should().Be("new-jwt-token");
        
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var invalidRefreshToken = "invalid-refresh-token";
        
        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken?)null); // No token found

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authenticationService.RefreshTokenAsync(invalidRefreshToken));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var expiredRefreshToken = "expired-refresh-token";
        
        var expiredTokenEntity = TestDataBuilder.RefreshToken()
            .WithId(1)
            .WithUserId(1)
            .WithToken(expiredRefreshToken)
            .WithExpiresAt(DateTime.UtcNow.AddDays(-1))
            .WithIsRevoked(false)
            .Build();

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(expiredTokenEntity);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authenticationService.RefreshTokenAsync(expiredRefreshToken));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var revokedRefreshToken = "revoked-refresh-token";
        
        var revokedTokenEntity = TestDataBuilder.RefreshToken()
            .WithId(1)
            .WithUserId(1)
            .WithToken(revokedRefreshToken)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .WithIsRevoked(true)
            .Build();

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(revokedTokenEntity);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authenticationService.RefreshTokenAsync(revokedRefreshToken));
    }

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var validToken = "valid-jwt-token";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1")
        };
        var mockPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var refreshTokens = new List<RefreshToken>
        {
            TestDataBuilder.RefreshToken().WithId(1).WithUserId(1).WithIsRevoked(false).Build(),
            TestDataBuilder.RefreshToken().WithId(2).WithUserId(1).WithIsRevoked(false).Build()
        };

        _mockJwtTokenService.Setup(x => x.ValidateToken(validToken))
            .Returns(mockPrincipal);
        _mockRefreshTokenRepository.Setup(x => x.GetActiveByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(refreshTokens);

        // Act
        var result = await _authenticationService.LogoutAsync(validToken);

        // Assert
        result.Should().BeTrue();
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid-jwt-token";
        
        _mockJwtTokenService.Setup(x => x.ValidateToken(invalidToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var result = await _authenticationService.LogoutAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidUserIdClaim_ReturnsFalse()
    {
        // Arrange
        var validToken = "valid-jwt-token";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid-id")
        };
        var mockPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _mockJwtTokenService.Setup(x => x.ValidateToken(validToken))
            .Returns(mockPrincipal);

        // Act
        var result = await _authenticationService.LogoutAsync(validToken);

        // Assert
        result.Should().BeFalse();
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region ForgotPasswordAsync Tests

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_ReturnsSuccessAndSendsEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand(Email: TestConstants.Users.DefaultEmail);
        var testUser = CreateTestUser(email: TestConstants.Users.DefaultEmail);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        _mockEmailService
            .Setup(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.ForgotPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("If the email exists, a password reset link has been sent.");

        _mockPasswordResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
            TestConstants.Users.DefaultEmail, 
            It.IsAny<string>(), 
            testUser.Username), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsSuccessButNoEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand(Email: "nonexistent@example.com");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authenticationService.ForgotPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("If the email exists, a password reset link has been sent.");

        _mockPasswordResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ReturnsSuccessAndUpdatesPassword()
    {
        // Arrange
        var resetToken = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: resetToken,
            NewPassword: "NewPassword123!"
        );
        
        var testUser = CreateTestUser();
        var passwordResetToken = TestDataBuilder.PasswordResetToken()
            .WithUserId(testUser.Id)
            .WithToken(resetToken)
            .WithExpiresAt(DateTime.UtcNow.AddHours(1))
            .Build();

        _mockPasswordResetTokenRepository
            .Setup(x => x.GetValidByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(passwordResetToken);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(testUser);

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password has been reset successfully.");

        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Id == testUser.Id)), Times.Once);
        _mockPasswordResetTokenRepository.Verify(x => x.UpdateAsync(It.Is<PasswordResetToken>(t => t.IsUsed == true)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand(
            Token: "invalid-token",
            NewPassword: "NewPassword123!"
        );

        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired reset token.");

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var resetToken = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: resetToken,
            NewPassword: "NewPassword123!"
        );
        
        var expiredToken = TestDataBuilder.PasswordResetToken()
            .WithUserId(1)
            .WithToken(resetToken)
            .WithExpiresAt(DateTime.UtcNow.AddHours(-1)) // Expired
            .Build();

        // Mock returns empty list because the token is expired (filtered out by the query)
        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired reset token.");

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithUsedToken_ReturnsFailure()
    {
        // Arrange
        var resetToken = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: resetToken,
            NewPassword: "NewPassword123!"
        );
        
        var usedToken = TestDataBuilder.PasswordResetToken()
            .WithUserId(1)
            .WithToken(resetToken)
            .WithExpiresAt(DateTime.UtcNow.AddHours(1))
            .WithIsUsed(true) // Already used
            .Build();

        // Mock returns empty list because the token is used (filtered out by the query)
        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired reset token.");

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    #endregion
}
