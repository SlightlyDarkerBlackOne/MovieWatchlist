using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistItemById;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Queries;

public class GetMyWatchlistItemByIdQueryHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly GetMyWatchlistItemByIdQueryHandler _handler;

    public GetMyWatchlistItemByIdQueryHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _handler = new GetMyWatchlistItemByIdQueryHandler(_mockWatchlistService.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsWatchlistItem()
    {
        var query = new GetMyWatchlistItemByIdQuery(WatchlistItemId: TestConstants.WatchlistItems.FirstItemId);
        var watchlistItem = WatchlistItem()
            .WithStatus(WatchlistStatus.Watched)
            .Build();

        _mockWatchlistService
            .Setup(x => x.GetWatchlistItemByIdAsync(query))
            .ReturnsAsync(watchlistItem);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(WatchlistStatus.Watched);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        var query = new GetMyWatchlistItemByIdQuery(WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId);

        _mockWatchlistService
            .Setup(x => x.GetWatchlistItemByIdAsync(query))
            .ReturnsAsync((WatchlistItem?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}

