using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class UpdateWatchlistItemCommandHandler : IRequestHandler<UpdateWatchlistItemCommand, Result<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWatchlistItemCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WatchlistItem>> Handle(
        UpdateWatchlistItemCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _watchlistService.UpdateWatchlistItemAsync(request);
        
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync();
        
        return result;
    }
}
