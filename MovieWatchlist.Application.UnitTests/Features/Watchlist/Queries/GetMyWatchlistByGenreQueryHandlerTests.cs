using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByGenre;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistByGenreQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistByGenreQueryHandler _handler;

    public GetMyWatchlistByGenreQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistByGenreQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidGenre_ReturnsWatchlistItems()
    {
        var query = new GetMyWatchlistByGenreQuery(Genre: GenreConstants.Action);
        var watchlistItems = new List<WatchlistItem>
        {
            WatchlistItem().Build(),
            WatchlistItem().Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByGenreAsync(query))
            .ReturnsAsync(watchlistItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingGenre_ReturnsEmptyList()
    {
        var query = new GetMyWatchlistByGenreQuery(Genre: GenreConstants.Comedy);

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByGenreAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

