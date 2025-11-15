using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Commands.UpdateWatchlistItem;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Commands;

public class UpdateWatchlistItemCommandHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateWatchlistItemCommandHandler _handler;

    public UpdateWatchlistItemCommandHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _handler = new UpdateWatchlistItemCommandHandler(
            _mockWatchlistService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        var command = new UpdateWatchlistItemCommand(
            WatchlistItemId: TestConstants.WatchlistItems.FirstItemId,
            Status: WatchlistStatus.Watched,
            IsFavorite: true,
            UserRating: 8,
            Notes: "Great movie!"
        );

        var watchlistItem = WatchlistItem()
            .WithStatus(WatchlistStatus.Watched)
            .WithIsFavorite(true)
            .WithUserRating(8)
            .Build();

        _mockWatchlistService
            .Setup(x => x.UpdateWatchlistItemAsync(command))
            .ReturnsAsync(Result<WatchlistItem>.Success(watchlistItem));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult()
    {
        var command = new UpdateWatchlistItemCommand(
            WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId,
            Status: WatchlistStatus.Watched
        );

        _mockWatchlistService
            .Setup(x => x.UpdateWatchlistItemAsync(command))
            .ReturnsAsync(Result<WatchlistItem>.Failure(TestConstants.ErrorMessages.WatchlistItemNotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.WatchlistItemNotFound);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}

