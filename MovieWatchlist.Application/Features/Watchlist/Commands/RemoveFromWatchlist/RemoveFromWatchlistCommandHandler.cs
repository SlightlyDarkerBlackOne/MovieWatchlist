using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Commands.RemoveFromWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public class RemoveFromWatchlistCommandHandler : IRequestHandler<RemoveFromWatchlistCommand, Result<bool>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromWatchlistCommandHandler(
        IWatchlistService watchlistService,
        IUnitOfWork unitOfWork)
    {
        _watchlistService = watchlistService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        RemoveFromWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _watchlistService.RemoveFromWatchlistAsync(request);
        
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync();
        
        return result;
    }
}

