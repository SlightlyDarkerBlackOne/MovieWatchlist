using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.DTOs;

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
        var watchlist = await _watchlistService.GetUserWatchlistAsync(userId);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/status/{status}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByStatus(int userId, WatchlistStatus status)
    {
        var watchlist = await _watchlistService.GetWatchlistByStatusAsync(userId, status);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/favorites")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetFavoriteMovies(int userId)
    {
        var favorites = await _watchlistService.GetFavoriteMoviesAsync(userId);
        return Ok(favorites);
    }

    [HttpGet("user/{userId}/statistics")]
    public async Task<ActionResult<WatchlistStatistics>> GetUserStatistics(int userId)
    {
        var statistics = await _watchlistService.GetUserStatisticsAsync(userId);
        return Ok(statistics);
    }

    [HttpGet("user/{userId}/recommendations")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetRecommendedMovies(int userId, [FromQuery] int limit = 10)
    {
        var recommendations = await _watchlistService.GetRecommendedMoviesAsync(userId, limit);
        return Ok(recommendations);
    }

    [HttpGet("user/{userId}/genre/{genre}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByGenre(int userId, string genre)
    {
        var watchlist = await _watchlistService.GetWatchlistByGenreAsync(userId, genre);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/year-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByYearRange(
        int userId, 
        [FromQuery] int startYear, 
        [FromQuery] int endYear)
    {
        var watchlist = await _watchlistService.GetWatchlistByYearRangeAsync(userId, startYear, endYear);
        return Ok(watchlist);
    }

    [HttpGet("user/{userId}/rating-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetWatchlistByRatingRange(
        int userId, 
        [FromQuery] double minRating, 
        [FromQuery] double maxRating)
    {
        var watchlist = await _watchlistService.GetWatchlistByRatingRangeAsync(userId, minRating, maxRating);
        return Ok(watchlist);
    }

    // CRUD Operations
    [HttpPost("user/{userId}/add")]
    public async Task<ActionResult<WatchlistItem>> AddToWatchlist(int userId, [FromBody] AddToWatchlistDto addDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var watchlistItem = await _watchlistService.AddToWatchlistAsync(userId, addDto.MovieId, addDto.Status, addDto);
            return CreatedAtAction(nameof(GetWatchlistItemById), 
                new { userId = userId, watchlistItemId = watchlistItem.Id }, watchlistItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> GetWatchlistItemById(int userId, int watchlistItemId)
    {
        var watchlistItem = await _watchlistService.GetWatchlistItemByIdAsync(userId, watchlistItemId);
        if (watchlistItem == null)
            return NotFound();

        return Ok(watchlistItem);
    }

    [HttpPut("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> UpdateWatchlistItem(
        int userId, 
        int watchlistItemId, 
        [FromBody] UpdateWatchlistItemDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var watchlistItem = await _watchlistService.UpdateWatchlistItemAsync(userId, watchlistItemId, updateDto);
        if (watchlistItem == null)
            return NotFound();

        return Ok(watchlistItem);
    }

    [HttpDelete("user/{userId}/item/{watchlistItemId}")]
    public async Task<ActionResult> RemoveFromWatchlist(int userId, int watchlistItemId)
    {
        var removed = await _watchlistService.RemoveFromWatchlistAsync(userId, watchlistItemId);
        if (!removed)
            return NotFound();

        return NoContent();
    }
} 