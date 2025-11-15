using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IAuthCookieService> _mockCookieService;
    private readonly Mock<ILogger<RegisterCommandHandler>> _mockLogger;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCookieService = new Mock<IAuthCookieService>();
        _mockLogger = new Mock<ILogger<RegisterCommandHandler>>();
        
        _handler = new RegisterCommandHandler(
            _mockAuthService.Object,
            _mockUnitOfWork.Object,
            _mockCookieService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword);

        var user = User()
            .WithId(1)
            .WithUsername(TestConstants.Users.DefaultUsername)
            .WithEmail(TestConstants.Users.DefaultEmail)
            .Build();

        var authResult = new AuthenticationResult(
            IsSuccess: true,
            Token: TestConstants.Jwt.TestJwtToken,
            RefreshToken: TestConstants.Jwt.TestRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddHours(1),
            User: new Application.Features.Auth.Common.UserInfo(1, TestConstants.Users.DefaultUsername, TestConstants.Users.DefaultEmail, DateTime.UtcNow)
        );

        _mockAuthService
            .Setup(x => x.RegisterUserAsync(command))
            .ReturnsAsync(Result<User>.Success(user));

        _mockAuthService
            .Setup(x => x.GenerateAuthenticationResultWithRefreshTokenAsync(user))
            .ReturnsAsync(authResult);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.User.Username.Should().Be(TestConstants.Users.DefaultUsername);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockCookieService.Verify(x => x.SetAuthCookies(
            TestConstants.Jwt.TestJwtToken,
            TestConstants.Jwt.TestRefreshToken,
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult()
    {
        var command = new RegisterCommand(
            Username: TestConstants.Users.DefaultUsername,
            Email: TestConstants.Users.DefaultEmail,
            Password: TestConstants.Users.DefaultPassword);

        _mockAuthService
            .Setup(x => x.RegisterUserAsync(command))
            .ReturnsAsync(Result<User>.Failure(TestConstants.ErrorMessages.UserAlreadyExists));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.UserAlreadyExists);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        _mockCookieService.Verify(x => x.SetAuthCookies(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Never);
    }
}

