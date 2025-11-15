using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistQueryHandler _handler;

    public GetMyWatchlistQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsWatchlistItems()
    {
        var query = new GetMyWatchlistQuery();
        var watchlistItems = new List<WatchlistItem>
        {
            WatchlistItem().WithStatus(WatchlistStatus.Planned).Build(),
            WatchlistItem().WithStatus(WatchlistStatus.Watched).Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetUserWatchlistAsync(query))
            .ReturnsAsync(watchlistItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithEmptyWatchlist_ReturnsEmptyList()
    {
        var query = new GetMyWatchlistQuery();

        _mockWatchlistService
            .Setup(x => x.GetUserWatchlistAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

