using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand, Result<Core.Models.WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;

    public AddToWatchlistCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Core.Models.WatchlistItem>> Handle(
        AddToWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _watchlistService.AddToWatchlistAsync(request);
        
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync();
        
        return result;
    }
}
