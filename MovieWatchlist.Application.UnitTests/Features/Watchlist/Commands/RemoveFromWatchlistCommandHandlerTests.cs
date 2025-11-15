using FluentAssertions;
using Moq;
using MovieWatchlist.Application.Features.Watchlist.Commands.RemoveFromWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Application.UnitTests.Features.Watchlist.Commands;

public class RemoveFromWatchlistCommandHandlerTests
{
    private readonly Mock<IWatchlistService> _mockWatchlistService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly RemoveFromWatchlistCommandHandler _handler;

    public RemoveFromWatchlistCommandHandlerTests()
    {
        _mockWatchlistService = new Mock<IWatchlistService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _handler = new RemoveFromWatchlistCommandHandler(
            _mockWatchlistService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        var command = new RemoveFromWatchlistCommand(WatchlistItemId: TestConstants.WatchlistItems.FirstItemId);

        _mockWatchlistService
            .Setup(x => x.RemoveFromWatchlistAsync(command))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult()
    {
        var command = new RemoveFromWatchlistCommand(WatchlistItemId: TestConstants.WatchlistItems.NonExistentItemId);

        _mockWatchlistService
            .Setup(x => x.RemoveFromWatchlistAsync(command))
            .ReturnsAsync(Result<bool>.Failure(TestConstants.ErrorMessages.WatchlistItemNotFound));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestConstants.ErrorMessages.WatchlistItemNotFound);
        
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}

