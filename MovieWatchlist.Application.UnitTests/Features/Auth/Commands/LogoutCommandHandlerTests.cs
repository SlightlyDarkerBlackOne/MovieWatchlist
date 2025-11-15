using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.Logout;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;
namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ITokenExtractor> _mockTokenExtractor;
    private readonly Mock<IAuthCookieService> _mockCookieService;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockTokenExtractor = new Mock<ITokenExtractor>();
        _mockCookieService = new Mock<IAuthCookieService>();
        
        _handler = new LogoutCommandHandler(
            _mockAuthService.Object,
            _mockTokenExtractor.Object,
            _mockCookieService.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ReturnsSuccessResult()
    {
        var command = new LogoutCommand();
        var token = TestConstants.Jwt.TestJwtToken;

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromCookie(CookieNames.AccessToken))
            .Returns(token);

        _mockAuthService
            .Setup(x => x.LogoutAsync(token))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Be(SuccessMessages.LogoutSuccess);
        
        _mockCookieService.Verify(x => x.ClearAuthCookies(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoToken_ClearsCookiesAndReturnsSuccess()
    {
        var command = new LogoutCommand();

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromCookie(CookieNames.AccessToken))
            .Returns(string.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        _mockAuthService.Verify(x => x.LogoutAsync(It.IsAny<string>()), Times.Never);
        _mockCookieService.Verify(x => x.ClearAuthCookies(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLogoutFailure_ReturnsFailureResult()
    {
        var command = new LogoutCommand();
        var token = TestConstants.Jwt.TestJwtToken;

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromCookie(CookieNames.AccessToken))
            .Returns(token);

        _mockAuthService
            .Setup(x => x.LogoutAsync(token))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.LogoutFailed);
        
        _mockCookieService.Verify(x => x.ClearAuthCookies(), Times.Never);
    }
}

