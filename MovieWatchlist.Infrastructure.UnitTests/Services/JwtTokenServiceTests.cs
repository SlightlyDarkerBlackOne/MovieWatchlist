using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Infrastructure.Services;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace MovieWatchlist.Infrastructure.UnitTests.Services;

/// <summary>
/// Unit tests for JwtTokenService
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenService _jwtTokenService;
    private readonly User _testUser;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = TestConstants.Jwt.TestSecretKey,
            ExpirationMinutes = TestConstants.Jwt.TestExpirationMinutes,
            Issuer = TestConstants.Jwt.TestIssuer,
            Audience = TestConstants.Jwt.TestAudience
        };

        var options = Options.Create(_jwtSettings);
        _jwtTokenService = new JwtTokenService(options);

        _testUser = User().Build();
    }

    #region GenerateToken Tests

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Act
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // JWT format: header.payload.signature
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ContainsCorrectClaims()
    {
        // Act
        var token = _jwtTokenService.GenerateToken(_testUser);
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(_testUser.Id.ToString());
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be(_testUser.Username.Value);
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(_testUser.Email.Value);
        principal.FindFirst(TestConstants.Jwt.UsernameClaimName)?.Value.Should().Be(_testUser.Username.Value);
        principal.FindFirst(TestConstants.Jwt.EmailClaimName)?.Value.Should().Be(_testUser.Email.Value);
    }

    [Fact]
    public void GenerateToken_WithValidUser_HasCorrectExpiration()
    {
        // Act
        var token = _jwtTokenService.GenerateToken(_testUser);
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        var expClaim = principal!.FindFirst(TestConstants.Jwt.ExpirationClaimName)?.Value;
        expClaim.Should().NotBeNull();

        var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!)).DateTime;
        var expectedExpTime = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        
        // Allow 1 minute tolerance for test execution time
        expTime.Should().BeCloseTo(expectedExpTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateToken_WithDifferentUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = User().WithUsername(TestConstants.Users.TestUser1Username).WithEmail(TestConstants.Users.TestUser1Email).Build();
        var user2 = User().WithId(TestConstants.Users.SecondUserId).WithUsername(TestConstants.Users.TestUser2Username).WithEmail(TestConstants.Users.TestUser2Email).Build();

        // Act
        var token1 = _jwtTokenService.GenerateToken(user1);
        var token2 = _jwtTokenService.GenerateToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ReturnsValidBase64String()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().MatchRegex(TestConstants.Jwt.Base64RegexPattern);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokens()
    {
        // Act
        var token1 = _jwtTokenService.GenerateRefreshToken();
        var token2 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsCorrectLength()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        // 32 bytes = 44 characters in base64 (including padding)
        refreshToken.Length.Should().Be(44);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var token = _jwtTokenService.GenerateToken(_testUser);

        // Act
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid.token.here")]
    [InlineData("not.a.jwt")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
    public void ValidateToken_WithInvalidToken_ReturnsNull(string invalidToken)
    {
        // Act
        var principal = _jwtTokenService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange - Create a manually constructed expired token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUser.Id.ToString()),
            new(ClaimTypes.Name, _testUser.Username),
            new(ClaimTypes.Email, _testUser.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow.AddMinutes(-TestConstants.Jwt.ExpiredTokenValidMinutesAgo),
            Expires = DateTime.UtcNow.AddMinutes(-TestConstants.Jwt.ExpiredTokenMinutesAgo),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var expiredToken = tokenHandler.CreateToken(tokenDescriptor);
        var expiredTokenString = tokenHandler.WriteToken(expiredToken);

        // Act
        var principal = _jwtTokenService.ValidateToken(expiredTokenString);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongSecret_ReturnsNull()
    {
        // Arrange
        var wrongSecretSettings = new JwtSettings
        {
            SecretKey = TestConstants.Jwt.WrongSecretKey,
            ExpirationMinutes = TestConstants.Jwt.TestExpirationMinutes,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        var wrongSecretService = new JwtTokenService(Options.Create(wrongSecretSettings));
        var tokenWithWrongSecret = wrongSecretService.GenerateToken(_testUser);

        // Act
        var principal = _jwtTokenService.ValidateToken(tokenWithWrongSecret);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongIssuer_ReturnsNull()
    {
        // Arrange
        var wrongIssuerSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            ExpirationMinutes = TestConstants.Jwt.TestExpirationMinutes,
            Issuer = TestConstants.Jwt.WrongIssuer,
            Audience = _jwtSettings.Audience
        };

        var wrongIssuerService = new JwtTokenService(Options.Create(wrongIssuerSettings));
        var tokenWithWrongIssuer = wrongIssuerService.GenerateToken(_testUser);

        // Act
        var principal = _jwtTokenService.ValidateToken(tokenWithWrongIssuer);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_WithWrongAudience_ReturnsNull()
    {
        // Arrange
        var wrongAudienceSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            ExpirationMinutes = TestConstants.Jwt.TestExpirationMinutes,
            Issuer = _jwtSettings.Issuer,
            Audience = TestConstants.Jwt.WrongAudience
        };

        var wrongAudienceService = new JwtTokenService(Options.Create(wrongAudienceSettings));
        var tokenWithWrongAudience = wrongAudienceService.GenerateToken(_testUser);

        // Act
        var principal = _jwtTokenService.ValidateToken(tokenWithWrongAudience);

        // Assert
        principal.Should().BeNull();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GenerateAndValidateToken_WithValidFlow_WorksCorrectly()
    {
        // Arrange
        var user = User()
            .WithId(TestConstants.Users.IntegrationTestUserId)
            .WithUsername(TestConstants.Users.IntegrationTestUsername)
            .WithEmail(TestConstants.Users.IntegrationTestEmail)
            .Build();

        // Act
        var token = _jwtTokenService.GenerateToken(user);
        var principal = _jwtTokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(TestConstants.Users.IntegrationTestUserId.ToString());
        principal.FindFirst(ClaimTypes.Name)?.Value.Should().Be(TestConstants.Users.IntegrationTestUsername);
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(TestConstants.Users.IntegrationTestEmail);
    }

    #endregion
}
