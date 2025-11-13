using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;

public class AddToWatchlistCommandHandler : IRequestHandler<AddToWatchlistCommand, Result<WatchlistItem>>
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

    public async Task<Result<WatchlistItem>> Handle(
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

