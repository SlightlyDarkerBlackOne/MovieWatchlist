using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class UpdateWatchlistItemCommandHandler : IRequestHandler<UpdateWatchlistItemCommand, Result<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWatchlistItemCommandHandler> _logger;

    public UpdateWatchlistItemCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateWatchlistItemCommandHandler> logger)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<WatchlistItem>> Handle(
        UpdateWatchlistItemCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing update watchlist item for user: {UserId}, watchlist item: {WatchlistItemId}", 
            request.UserId, request.WatchlistItemId);

        var result = await _watchlistService.UpdateWatchlistItemAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update watchlist item: {Error}", result.Error);
            return result;
        }

        // Save changes to persist the update
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Successfully updated watchlist item {WatchlistItemId} for user {UserId}", 
            request.WatchlistItemId, request.UserId);
        
        return result;
    }
}
