using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Auth.Queries.GetCurrentUser;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Application.UnitTests.Features.Auth.Queries;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        _handler = new GetCurrentUserQueryHandler(
            _mockCurrentUserService.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUser_ReturnsUserInfo()
    {
        var query = new GetCurrentUserQuery();
        var userId = TestConstants.Users.DefaultUserId;
        var user = User()
            .WithId(userId)
            .WithUsername(TestConstants.Users.DefaultUsername)
            .WithEmail(TestConstants.Users.DefaultEmail)
            .Build();

        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Username.Should().Be(TestConstants.Users.DefaultUsername);
        result.Value.Email.Should().Be(TestConstants.Users.DefaultEmail);
    }

    [Fact]
    public async Task Handle_WithUnauthenticatedUser_ReturnsFailure()
    {
        var query = new GetCurrentUserQuery();

        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns((int?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.UserNotAuthenticated);
        
        _mockUserRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        var query = new GetCurrentUserQuery();
        var userId = TestConstants.Users.DefaultUserId;

        _mockCurrentUserService
            .Setup(x => x.UserId)
            .Returns(userId);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorMessages.UserNotFound);
    }
}

