using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetUserWatchlist(int userId)
    {
        var query = new GetUserWatchlistQuery(UserId: userId);
        var watchlist = await _watchlistService.GetUserWatchlistAsync(query);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/status/{status}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByStatus(int userId, WatchlistStatus status)
    {
        var query = new GetWatchlistByStatusQuery(UserId: userId, Status: status);
        var watchlist = await _watchlistService.GetWatchlistByStatusAsync(query);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/favorites")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetFavoriteMovies(int userId)
    {
        var query = new GetFavoriteMoviesQuery(UserId: userId);
        var favorites = await _watchlistService.GetFavoriteMoviesAsync(query);
        return Ok(favorites);
    }

    [HttpGet("user/{userId}/statistics")]
    public async Task<ActionResult<WatchlistStatistics>> GetUserStatistics(int userId)
    {
        var query = new GetUserStatisticsQuery(UserId: userId);
        var statistics = await _watchlistService.GetUserStatisticsAsync(query);
        return Ok(statistics);
    }

    [HttpGet("user/{userId}/recommendations")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetRecommendedMovies(int userId, [FromQuery] int limit = 10)
    {
        var query = new GetRecommendedMoviesQuery(UserId: userId, Limit: limit);
        var recommendations = await _watchlistService.GetRecommendedMoviesAsync(query);
        return Ok(recommendations);
    }

    [HttpGet("user/{userId}/genre/{genre}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByGenre(int userId, string genre)
    {
        var query = new GetWatchlistByGenreQuery(UserId: userId, Genre: genre);
        var watchlist = await _watchlistService.GetWatchlistByGenreAsync(query);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/year-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByYearRange(
        int userId, 
        [FromQuery] int startYear, 
        [FromQuery] int endYear)
    {
        var query = new GetWatchlistByYearRangeQuery(UserId: userId, StartYear: startYear, EndYear: endYear);
        var watchlist = await _watchlistService.GetWatchlistByYearRangeAsync(query);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/rating-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByRatingRange(
        int userId, 
        [FromQuery] double minRating, 
        [FromQuery] double maxRating)
    {
        var query = new GetWatchlistByRatingRangeQuery(UserId: userId, MinRating: minRating, MaxRating: maxRating);
        var watchlist = await _watchlistService.GetWatchlistByRatingRangeAsync(query);
        return Ok(watchlist);
    }

    // CRUD Operations
    [HttpPost("user/{userId}/add")]
    public async Task<ActionResult<WatchlistItem>> AddToWatchlist(int userId, [FromBody] AddToWatchlistDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new AddToWatchlistCommand(
            UserId: userId,
            MovieId: dto.MovieId,
            Status: dto.Status,
            Notes: dto.Notes
        );

        var result = await _watchlistService.AddToWatchlistAsync(command);
        
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetWatchlistItemById), 
            new { userId = userId, watchlistItemId = result.Value!.Id }, result.Value);
    }

    [HttpGet("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> GetWatchlistItemById(int userId, int watchlistItemId)
    {
        var query = new GetWatchlistItemByIdQuery(UserId: userId, WatchlistItemId: watchlistItemId);
        var watchlistItem = await _watchlistService.GetWatchlistItemByIdAsync(query);
        if (watchlistItem == null)
            return NotFound();

        return Ok(watchlistItem);
    }

    [HttpPut("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> UpdateWatchlistItem(
        int userId, 
        int watchlistItemId, 
        [FromBody] UpdateWatchlistItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new UpdateWatchlistItemCommand(
            UserId: userId,
            WatchlistItemId: watchlistItemId,
            Status: dto.Status,
            IsFavorite: dto.IsFavorite,
            UserRating: dto.UserRating,
            Notes: dto.Notes,
            WatchedDate: dto.WatchedDate
        );

        var result = await _watchlistService.UpdateWatchlistItemAsync(command);
        
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult> RemoveFromWatchlist(int userId, int watchlistItemId)
    {
        var command = new RemoveFromWatchlistCommand(UserId: userId, WatchlistItemId: watchlistItemId);
        var result = await _watchlistService.RemoveFromWatchlistAsync(command);
        
        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }
} 