using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IAuthCookieService> _mockCookieService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockCookieService = new Mock<IAuthCookieService>();
        
        _handler = new LoginCommandHandler(
            _mockAuthService.Object,
            _mockCookieService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccessResult()
    {
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultUsername,
            Password: TestConstants.Users.DefaultPassword);

        var authResultValue = new AuthenticationResult(
            IsSuccess: true,
            Token: TestConstants.Jwt.TestJwtToken,
            RefreshToken: TestConstants.Jwt.TestRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: new Application.Features.Auth.Common.UserInfo(1, TestConstants.Users.DefaultUsername, TestConstants.Users.DefaultEmail, DateTime.UtcNow)
        );

        _mockAuthService
            .Setup(x => x.LoginAsync(command))
            .ReturnsAsync(Result<AuthenticationResult>.Success(authResultValue));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.User.Username.Should().Be(TestConstants.Users.DefaultUsername);
        
        _mockCookieService.Verify(x => x.SetAuthCookies(
            TestConstants.Jwt.TestJwtToken,
            TestConstants.Jwt.TestRefreshToken,
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ReturnsFailureResult()
    {
        var command = new LoginCommand(
            UsernameOrEmail: TestConstants.Users.DefaultUsername,
            Password: TestConstants.Users.WrongPassword);

        _mockAuthService
            .Setup(x => x.LoginAsync(command))
            .ReturnsAsync(Result<AuthenticationResult>.Failure(TestConstants.ErrorMessages.InvalidCredentials));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.InvalidCredentials);
        
        _mockCookieService.Verify(x => x.SetAuthCookies(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
    }
}

