using System.ComponentModel.DataAnnotations;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.DTOs;

public class UpdateWatchlistItemDto
{
    public WatchlistStatus? Status { get; set; }
    public bool? IsFavorite { get; set; }
    
    [Range(1, 10, ErrorMessage = "User rating must be between 1 and 10")]
    public int? UserRating { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
    public DateTime? WatchedDate { get; set; }
}

public class AddToWatchlistDto
{
    [Required(ErrorMessage = "MovieId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a positive number")]
    public int MovieId { get; set; }
    
    public WatchlistStatus Status { get; set; } = WatchlistStatus.Planned;
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
