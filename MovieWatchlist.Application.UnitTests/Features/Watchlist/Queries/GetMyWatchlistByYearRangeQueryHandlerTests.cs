using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByYearRange;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistByYearRangeQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistByYearRangeQueryHandler _handler;

    public GetMyWatchlistByYearRangeQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistByYearRangeQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidYearRange_ReturnsWatchlistItems()
    {
        var query = new GetMyWatchlistByYearRangeQuery(StartYear: 2020, EndYear: 2023);
        var watchlistItems = new List<WatchlistItem>
        {
            WatchlistItem().Build(),
            WatchlistItem().Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByYearRangeAsync(query))
            .ReturnsAsync(watchlistItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingYears_ReturnsEmptyList()
    {
        var query = new GetMyWatchlistByYearRangeQuery(StartYear: 1990, EndYear: 1995);

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByYearRangeAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

