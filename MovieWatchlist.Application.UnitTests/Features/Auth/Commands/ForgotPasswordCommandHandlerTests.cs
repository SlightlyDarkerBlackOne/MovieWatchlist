using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Tests.Shared.Infrastructure;
namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _mockLogger;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<ForgotPasswordCommandHandler>>();
        
        _handler = new ForgotPasswordCommandHandler(
            _mockAuthService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidEmail_ReturnsSuccessResult()
    {
        var command = new ForgotPasswordCommand(Email: TestConstants.Users.DefaultEmail);
        var response = new PasswordResetResponse(
            Success: true,
            Message: ErrorMessages.PasswordResetEmailSent
        );

        _mockAuthService
            .Setup(x => x.ForgotPasswordAsync(command))
            .ReturnsAsync(response);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Success.Should().BeTrue();
        result.Value.Message.Should().Be(ErrorMessages.PasswordResetEmailSent);
    }
}

