using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class RemoveFromWatchlistCommandHandler : IRequestHandler<RemoveFromWatchlistCommand, Result<bool>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromWatchlistCommandHandler> _logger;

    public RemoveFromWatchlistCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromWatchlistCommandHandler> logger)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RemoveFromWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing remove from watchlist for user: {UserId}, watchlist item: {WatchlistItemId}", 
            request.UserId, request.WatchlistItemId);

        var result = await _watchlistService.RemoveFromWatchlistAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to remove item from watchlist: {Error}", result.Error);
            return result;
        }

        // Save changes to persist the removal
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Successfully removed watchlist item {WatchlistItemId} for user {UserId}", 
            request.WatchlistItemId, request.UserId);
        
        return result;
    }
}
