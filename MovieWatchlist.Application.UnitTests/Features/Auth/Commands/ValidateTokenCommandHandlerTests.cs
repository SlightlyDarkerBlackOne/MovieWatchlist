using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Commands.ValidateToken;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Application.UnitTests.Features.Auth.Commands;

public class ValidateTokenCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ITokenExtractor> _mockTokenExtractor;
    private readonly ValidateTokenCommandHandler _handler;

    public ValidateTokenCommandHandlerTests()
    {
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockTokenExtractor = new Mock<ITokenExtractor>();
        
        _handler = new ValidateTokenCommandHandler(
            _mockAuthService.Object,
            _mockTokenExtractor.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ReturnsSuccessResult()
    {
        var command = new ValidateTokenCommand();
        var token = TestConstants.Jwt.TestJwtToken;

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromHeader())
            .Returns(token);

        _mockAuthService
            .Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ReturnsSuccessWithIsValidFalse()
    {
        var command = new ValidateTokenCommand();
        var token = TestConstants.Jwt.InvalidJwtToken;

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromHeader())
            .Returns(token);

        _mockAuthService
            .Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNoToken_ReturnsFailureResult()
    {
        var command = new ValidateTokenCommand();

        _mockTokenExtractor
            .Setup(x => x.ExtractTokenFromHeader())
            .Returns(string.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.TokenNotProvided);
        
        _mockAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
    }
}

