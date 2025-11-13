using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Tests.TestDataBuilders;
using MovieWatchlist.Tests.Infrastructure;
using static MovieWatchlist.Tests.TestDataBuilders.TestDataBuilder;
using System.Security.Claims;
using Xunit;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordResetTokenRepository> _mockPasswordResetTokenRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
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

        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null); // No existing users
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<Username>()))
            .ReturnsAsync((User?)null); // No existing users
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(TestConstants.Jwt.TestJwtToken);

        // Act
        var userResult = await _authenticationService.RegisterUserAsync(command);

        // Assert
        userResult.IsSuccess.Should().BeTrue();
        userResult.Value.Should().NotBeNull();
        userResult.Value!.Username.Value.Should().Be(TestConstants.Users.DefaultUsername);
        userResult.Value.Email.Value.Should().Be(TestConstants.Users.DefaultEmail);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);

        // Test GenerateAuthenticationResult separately
        var authResult = _authenticationService.GenerateAuthenticationResult(userResult.Value!);
        authResult.IsSuccess.Should().BeTrue();
        authResult.Token.Should().Be(TestConstants.Jwt.TestJwtToken);
        authResult.User.Should().NotBeNull();
        authResult.User!.Username.Should().Be(TestConstants.Users.DefaultUsername);
        authResult.User.Email.Should().Be(TestConstants.Users.DefaultEmail);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.ExistingEmail,
            Password: TestConstants.Users.ValidPassword1
        );

        var existingUser = User().WithEmail(TestConstants.Users.ExistingEmail).Build();
        var email = Email.Create(command.Email).Value!;
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser); // Existing user with same email

        // Act
        var result = await _authenticationService.RegisterUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.EmailAlreadyRegistered);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.ExistingUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.ValidPassword1
        );

        // First call returns null (email check), second call returns existing user (username check)
        var email = Email.Create(command.Email).Value!;
        var username = Username.Create(command.Username).Value!;
        _mockUserRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null); // No existing email
        _mockUserRepository.Setup(x => x.GetByUsernameAsync(username))
            .ReturnsAsync(User().WithUsername(TestConstants.Users.ExistingUsername).Build());

        // Act
        var result = await _authenticationService.RegisterUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.UsernameAlreadyTaken);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.WeakPassword
        );

        // Act
        var result = await _authenticationService.RegisterUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Password.InvalidFormatMessage);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidEmail_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.InvalidEmail,
            Password: TestConstants.Users.DefaultPassword
        );

        // Act
        var result = await _authenticationService.RegisterUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Email.InvalidFormatMessage);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidUsername_ReturnsFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            Username: TestConstants.Users.InvalidUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword
        );

        // Act
        var result = await _authenticationService.RegisterUserAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ValidationConstants.Username.InvalidFormatMessage);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
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

        var user = User().WithPasswordHash(new PasswordHasher().HashPassword(TestConstants.Users.DefaultPassword)).Build();
        typeof(User).GetProperty("Id")!.SetValue(user, 1);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<Username>()))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(TestConstants.Jwt.TestJwtToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns(TestConstants.Jwt.TestRefreshToken);

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be(TestConstants.Jwt.TestJwtToken);
        result.Value.RefreshToken.Should().Be(TestConstants.Jwt.TestRefreshToken);
        result.Value.User.Should().NotBeNull();
        result.Value.User!.Username.Should().Be(TestConstants.Users.DefaultUsername);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailureResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.NonexistentUsername,
            Password: TestConstants.Users.ValidPassword1
        );

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<Username>()))
            .ReturnsAsync((User?)null); // No user found
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null); // No user found

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.InvalidCredentials);
        result.Value.Should().BeNull();

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFailureResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultUsername,
            Password: TestConstants.Users.WrongPassword
        );

        var user = User().WithPasswordHash(new PasswordHasher().HashPassword(TestConstants.Users.CorrectPassword)).Build();
        typeof(User).GetProperty("Id")!.SetValue(user, 1);

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<Username>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorMessages.InvalidCredentials);
        result.Value.Should().BeNull(); // Value should be null for failure results

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmail_ReturnsSuccessResult()
    {
        // Arrange
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword
        );

        var user = User().WithPasswordHash(new PasswordHasher().HashPassword(TestConstants.Users.DefaultPassword)).Build();
        typeof(User).GetProperty("Id")!.SetValue(user, 1);

        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(TestConstants.Jwt.TestJwtToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns(TestConstants.Jwt.TestRefreshToken);

        // Act
        var result = await _authenticationService.LoginAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be(TestConstants.Jwt.TestJwtToken);
        result.Value.User.Should().NotBeNull();
        result.Value.User!.Email.Should().Be(TestConstants.Users.DefaultEmail);
    }

    #endregion

    #region ValidateTokenAsync Tests

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var validToken = TestConstants.Jwt.ValidJwtToken;
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
        var invalidToken = TestConstants.Jwt.InvalidJwtToken;
        
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
        var validRefreshToken = TestConstants.Jwt.ValidRefreshToken;
        var user = User().Build();
        var refreshTokenEntity = RefreshToken().WithToken(validRefreshToken).Build();
        typeof(RefreshToken).GetProperty("Id")!.SetValue(refreshTokenEntity, 1);
        typeof(RefreshToken).GetProperty("IsRevoked")!.SetValue(refreshTokenEntity, false);

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(refreshTokenEntity);
        _mockUserRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);
        _mockJwtTokenService.Setup(x => x.GenerateToken(user))
            .Returns(TestConstants.Jwt.NewJwtToken);
        _mockJwtTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns(TestConstants.Jwt.NewRefreshToken);

        // Act
        var result = await _authenticationService.RefreshTokenAsync(validRefreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be(TestConstants.Jwt.NewJwtToken);
        result.RefreshToken.Should().Be(TestConstants.Jwt.NewRefreshToken);
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(TestConstants.Users.DefaultUsername);
        result.User.Email.Should().Be(TestConstants.Users.DefaultEmail);
        
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var invalidRefreshToken = TestConstants.Jwt.InvalidRefreshToken;
        
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
        var expiredRefreshToken = TestConstants.Jwt.ExpiredRefreshToken;
        
        var expiredTokenEntity = RefreshToken()
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
        var revokedRefreshToken = TestConstants.Jwt.RevokedRefreshToken;
        
        var revokedTokenEntity = RefreshToken().WithToken(revokedRefreshToken).WithIsRevoked(true).Build();
        typeof(RefreshToken).GetProperty("Id")!.SetValue(revokedTokenEntity, 1);
        typeof(RefreshToken).GetProperty("IsRevoked")!.SetValue(revokedTokenEntity, true);

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
        var validToken = TestConstants.Jwt.ValidJwtToken;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1")
        };
        var mockPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var refreshTokens = new List<RefreshToken>
        {
            CreateRefreshToken(1, 1, false),
            CreateRefreshToken(2, 1, false)
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
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = TestConstants.Jwt.InvalidJwtToken;
        
        _mockJwtTokenService.Setup(x => x.ValidateToken(invalidToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var result = await _authenticationService.LogoutAsync(invalidToken);

        // Assert
        result.Should().BeFalse();
        _mockRefreshTokenRepository.Verify(x => x.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidUserIdClaim_ReturnsFalse()
    {
        // Arrange
        var validToken = TestConstants.Jwt.ValidJwtToken;
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
    }

    #endregion

    #region ForgotPasswordAsync Tests

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_ReturnsSuccessAndSendsEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand(Email: TestConstants.Users.DefaultEmail);
        var testUser = User().Build();

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
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
        result.Message.Should().Be(ErrorMessages.PasswordResetEmailSent);

        _mockPasswordResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(
            TestConstants.Users.DefaultEmail, 
            It.IsAny<string>(), 
            testUser.Username), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistentEmail_ReturnsSuccessButNoEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand(Email: TestConstants.Users.NonexistentEmail);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authenticationService.ForgotPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(ErrorMessages.PasswordResetEmailSent);

        _mockPasswordResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        _mockEmailService.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.NewPassword
        );
        
        var testUser = User().Build();
        var passwordResetToken = PasswordResetToken().WithUserId(testUser.Id).WithToken(resetToken).Build();

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
        result.Message.Should().Be(ErrorMessages.PasswordResetSuccess);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => u.Id == testUser.Id)), Times.Once);
        _mockPasswordResetTokenRepository.Verify(x => x.UpdateAsync(It.Is<PasswordResetToken>(t => t.IsUsed == true)), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand(
            Token: TestConstants.Jwt.InvalidToken,
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.NewPassword
        );

        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessages.InvalidOrExpiredResetToken);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ReturnsFailure()
    {
        // Arrange
        var resetToken = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: resetToken,
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.NewPassword
        );
        
        var expiredToken = PasswordResetToken()
            .WithToken(resetToken)
            .WithExpiresAt(DateTime.UtcNow.AddHours(-1))
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
        result.Message.Should().Be(ErrorMessages.InvalidOrExpiredResetToken);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithUsedToken_ReturnsFailure()
    {
        // Arrange
        var resetToken = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: resetToken,
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.NewPassword
        );
        
        var usedToken = PasswordResetToken().WithToken(resetToken).WithIsUsed(true).Build();
        typeof(PasswordResetToken).GetProperty("IsUsed")!.SetValue(usedToken, true);

        // Mock returns empty list because the token is used (filtered out by the query)
        _mockPasswordResetTokenRepository
            .Setup(x => x.GetUnusedByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _authenticationService.ResetPasswordAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorMessages.InvalidOrExpiredResetToken);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static RefreshToken CreateRefreshToken(int id, int userId, bool isRevoked)
    {
        var token = RefreshToken().WithUserId(userId).WithToken($"token-{id}").Build();
        typeof(RefreshToken).GetProperty("Id")!.SetValue(token, id);
        typeof(RefreshToken).GetProperty("IsRevoked")!.SetValue(token, isRevoked);
        return token;
    }

    #endregion
}
