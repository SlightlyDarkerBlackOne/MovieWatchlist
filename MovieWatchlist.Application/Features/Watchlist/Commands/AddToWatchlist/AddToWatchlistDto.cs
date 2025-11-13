using System.ComponentModel.DataAnnotations;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistDto(
    [Required(ErrorMessage = "MovieId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a positive number")]
    int MovieId,
    
    WatchlistStatus Status = WatchlistStatus.Planned,
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    string? Notes = null
);

