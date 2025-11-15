using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;

namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Commands;

public class AddToWatchlistCommandHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AddToWatchlistCommandHandler _handler;

    public AddToWatchlistCommandHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _handler = new AddToWatchlistCommandHandler(
            _mockWatchlistService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        var command = new AddToWatchlistCommand(
            MovieId: TestConstants.Movies.DefaultTmdbId,
            Status: WatchlistStatus.Planned,
            Notes: "Want to watch");

        var movie = Movie()
            .WithTmdbId(TestConstants.Movies.DefaultTmdbId)
            .Build();

        var watchlistItem = WatchlistItem()
            .WithUserId(TestConstants.Users.DefaultUserId)
            .WithMovie(movie)
            .WithStatus(WatchlistStatus.Planned)
            .Build();

        _mockWatchlistService
            .Setup(x => x.AddToWatchlistAsync(command))
            .ReturnsAsync(Result<WatchlistItem>.Success(watchlistItem));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.MovieId.Should().Be(movie.Id);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult()
    {
        var command = new AddToWatchlistCommand(
            MovieId: TestConstants.Movies.DefaultTmdbId,
            Status: WatchlistStatus.Planned);

        _mockWatchlistService
            .Setup(x => x.AddToWatchlistAsync(command))
            .ReturnsAsync(Result<WatchlistItem>.Failure(TestConstants.ErrorMessages.MovieNotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.MovieNotFound);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}

