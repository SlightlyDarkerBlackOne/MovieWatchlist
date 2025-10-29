using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand, Result<Core.Models.WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToWatchlistCommandHandler> _logger;

    public AddToWatchlistCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork,
        ILogger<AddToWatchlistCommandHandler> logger)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Core.Models.WatchlistItem>> Handle(
        AddToWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing add to watchlist for user: {UserId}, movie: {MovieId}", 
            request.UserId, request.MovieId);

        var result = await _watchlistService.AddToWatchlistAsync(request);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to add movie to watchlist: {Error}", result.Error);
            return result;
        }

        // Save changes to persist the watchlist item and any new movies
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Successfully added movie {MovieId} to watchlist for user {UserId}", 
            request.MovieId, request.UserId);
        
        return result;
    }
}
