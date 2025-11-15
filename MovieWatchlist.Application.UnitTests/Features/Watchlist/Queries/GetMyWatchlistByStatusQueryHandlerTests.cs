using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByStatus;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistByStatusQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistByStatusQueryHandler _handler;

    public GetMyWatchlistByStatusQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistByStatusQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidStatus_ReturnsWatchlistItems()
    {
        var query = new GetMyWatchlistByStatusQuery(Status: WatchlistStatus.Watched);
        var watchlistItems = new List<WatchlistItem>
        {
            WatchlistItem().WithStatus(WatchlistStatus.Watched).Build(),
            WatchlistItem().WithStatus(WatchlistStatus.Watched).Build()
        };

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByStatusAsync(query))
            .ReturnsAsync(watchlistItems);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(item => item.Status.Should().Be(WatchlistStatus.Watched));
    }

    [Fact]
    public async Task Handle_WithNoMatchingStatus_ReturnsEmptyList()
    {
        var query = new GetMyWatchlistByStatusQuery(Status: WatchlistStatus.Planned);

        _mockWatchlistService
            .Setup(x => x.GetWatchlistByStatusAsync(query))
            .ReturnsAsync(new List<WatchlistItem>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}

