using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Tests.Shared.Infrastructure;
namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _mockLogger;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockLogger = new Mock<ILogger<ResetPasswordCommandHandler>>();
        
        _handler = new ResetPasswordCommandHandler(
            _mockAuthService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithMatchingPasswords_ReturnsSuccessResult()
    {
        var token = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.NewPassword
        );

        var response = new MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword.PasswordResetResponse(
            Success: true,
            Message: ErrorMessages.PasswordResetSuccess
        );

        _mockAuthService
            .Setup(x => x.ResetPasswordAsync(command))
            .ReturnsAsync(response);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMismatchedPasswords_ReturnsFailureResult()
    {
        var token = Guid.NewGuid().ToString();
        var command = new ResetPasswordCommand(
            Token: token,
            NewPassword: TestConstants.Users.NewPassword,
            ConfirmPassword: TestConstants.Users.ValidPassword1
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.PasswordMismatch);
        
        _mockAuthService.Verify(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordCommand>()), Times.Never);
    }
}

