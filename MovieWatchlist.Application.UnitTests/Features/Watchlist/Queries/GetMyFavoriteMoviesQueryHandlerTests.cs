using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyFavoriteMovies;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyFavoriteMoviesQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyFavoriteMoviesQueryHandler _handler;

    public GetMyFavoriteMoviesQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyFavoriteMoviesQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsFavoriteMovies()
    {
        var query = new GetMyFavoriteMoviesQuery();
        var favoriteItems = new List<WatchlistItem>
        {
            WatchlistItem().WithIsFavorite(true).Build(),
            WatchlistItem().WithIsFavorite(true).Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetFavoriteMoviesAsync(query))
            .ReturnsAsync(favoriteItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(item => item.IsFavorite.Should().BeTrue());
    }

    [Fact]
    public async Task Handle_WithNoFavorites_ReturnsEmptyList()
    {
        var query = new GetMyFavoriteMoviesQuery();

        _mockWatchlistService
            .Setup(x => x.GetFavoriteMoviesAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

