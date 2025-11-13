using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.UpdateWatchlistItem;

public record UpdateWatchlistItemCommand(
    int WatchlistItemId,
    WatchlistStatus? Status = null,
    bool? IsFavorite = null,
    
    [Range(1, 10, ErrorMessage = "User rating must be between 1 and 10")]
    int? UserRating = null,
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    string? Notes = null,
    DateTime? WatchedDate = null
) : IRequest<Result<WatchlistItem>>;

