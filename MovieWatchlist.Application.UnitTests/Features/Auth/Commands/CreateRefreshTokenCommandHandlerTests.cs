using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Tests.Shared.Infrastructure;
namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class CreateRefreshTokenCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly CreateRefreshTokenCommandHandler _handler;

    public CreateRefreshTokenCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _handler = new CreateRefreshTokenCommandHandler(_mockAuthService.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ReturnsSuccessResult()
    {
        var command = new CreateRefreshTokenCommand(UserId: TestConstants.Users.DefaultUserId);
        var refreshTokenResult = new RefreshTokenResult(
            Token: TestConstants.Jwt.TestRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddDays(7)
        );

        _mockAuthService
            .Setup(x => x.CreateRefreshTokenAsync(TestConstants.Users.DefaultUserId))
            .ReturnsAsync(Result<RefreshTokenResult>.Success(refreshTokenResult));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be(TestConstants.Jwt.TestRefreshToken);
    }

    [Fact]
    public async Task Handle_WithInvalidUserId_ReturnsFailureResult()
    {
        var command = new CreateRefreshTokenCommand(UserId: TestConstants.Users.NonExistentUserId);

        _mockAuthService
            .Setup(x => x.CreateRefreshTokenAsync(TestConstants.Users.NonExistentUserId))
            .ReturnsAsync(Result<RefreshTokenResult>.Failure(ErrorMessages.UserNotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.UserNotFound);
    }
}

