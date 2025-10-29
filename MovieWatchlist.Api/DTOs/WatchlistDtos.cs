using System.ComponentModel.DataAnnotations;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Api.DTOs;

public record AddToWatchlistDto(
    [Required(ErrorMessage = "MovieId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a positive number")]
    int MovieId,
    
    WatchlistStatus Status = WatchlistStatus.Planned,
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    string? Notes = null
);

public record UpdateWatchlistItemDto(
    WatchlistStatus? Status = null,
    bool? IsFavorite = null,
    
    [Range(1, 10, ErrorMessage = "User rating must be between 1 and 10")]
    int? UserRating = null,
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    string? Notes = null,
    
    DateTime? WatchedDate = null
);


