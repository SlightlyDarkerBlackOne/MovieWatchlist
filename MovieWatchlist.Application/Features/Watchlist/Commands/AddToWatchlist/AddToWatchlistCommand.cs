using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistCommand(
    [Required(ErrorMessage = "MovieId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a positive number")]
    int MovieId,
    WatchlistStatus Status = WatchlistStatus.Planned,
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    string? Notes = null
) : IRequest<Result<WatchlistItem>>;

