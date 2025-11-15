using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByRatingRange;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistByRatingRangeQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistByRatingRangeQueryHandler _handler;

    public GetMyWatchlistByRatingRangeQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistByRatingRangeQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRatingRange_ReturnsWatchlistItems()
    {
        var query = new GetMyWatchlistByRatingRangeQuery(MinRating: 7.0, MaxRating: 10.0);
        var watchlistItems = new List<WatchlistItem>
        {
            WatchlistItem().WithUserRating(8).Build(),
            WatchlistItem().WithUserRating(9).Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByRatingRangeAsync(query))
            .ReturnsAsync(watchlistItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingRatings_ReturnsEmptyList()
    {
        var query = new GetMyWatchlistByRatingRangeQuery(MinRating: 9.0, MaxRating: 10.0);

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByRatingRangeAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

