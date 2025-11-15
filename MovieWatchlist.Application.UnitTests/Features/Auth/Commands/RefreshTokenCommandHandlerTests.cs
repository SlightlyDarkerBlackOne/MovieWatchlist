using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;
namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ITokenExtractor> _mockTokenExtractor;
    private readonly Mock<IAuthCookieService> _mockCookieService;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockTokenExtractor = new Mock<ITokenExtractor>();
        _mockCookieService = new Mock<IAuthCookieService>();
        
        _handler = new RefreshTokenCommandHandler(
            _mockAuthService.Object,
            _mockTokenExtractor.Object,
            _mockCookieService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsSuccessResult()
    {
        var command = new RefreshTokenCommand();
        var refreshToken = TestConstants.Jwt.TestRefreshToken;
        var authResult = new AuthenticationResult(
            IsSuccess: true,
            Token: TestConstants.Jwt.NewJwtToken,
            RefreshToken: TestConstants.Jwt.NewRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: new UserInfo(1, TestConstants.Users.DefaultUsername, TestConstants.Users.DefaultEmail, DateTime.UtcNow)
        );

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromCookie(CookieNames.RefreshToken))
            .Returns(refreshToken);

        _mockAuthService
            .Setup(x => x.RefreshTokenAsync(refreshToken))
            .ReturnsAsync(authResult);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.User.Username.Should().Be(TestConstants.Users.DefaultUsername);
        
        _mockCookieService.Verify(x => x.SetAuthCookies(
            TestConstants.Jwt.NewJwtToken,
            TestConstants.Jwt.NewRefreshToken,
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoRefreshToken_ReturnsFailureResult()
    {
        var command = new RefreshTokenCommand();

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromCookie(CookieNames.RefreshToken))
            .Returns(string.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.RefreshTokenNotProvided);
        
        _mockAuthService.Verify(x => x.RefreshTokenAsync(It.IsAny<string>()), Times.Never);
        _mockCookieService.Verify(x => x.SetAuthCookies(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
    }
}

