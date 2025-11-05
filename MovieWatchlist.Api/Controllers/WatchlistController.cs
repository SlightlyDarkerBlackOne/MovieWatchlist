using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Models;
using MediatR;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchlistController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public WatchlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me/watchlist")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyWatchlist(CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistQuery();
        var watchlist = await _mediator.Send(query, cancellationToken);
        return Ok(watchlist);
    }

    [HttpGet("me/watchlist/status/{status}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyWatchlistByStatus(
        WatchlistStatus status, 
        CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistByStatusQuery(Status: status);
        var watchlist = await _mediator.Send(query, cancellationToken);
        return Ok(watchlist);
    }

    [HttpGet("me/watchlist/favorites")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyFavoriteMovies(CancellationToken cancellationToken)
    {
        var query = new GetMyFavoriteMoviesQuery();
        var favorites = await _mediator.Send(query, cancellationToken);
        return Ok(favorites);
    }

    [HttpGet("me/watchlist/statistics")]
    public async Task<ActionResult<WatchlistStatistics>> GetMyStatistics(CancellationToken cancellationToken)
    {
        var query = new GetMyStatisticsQuery();
        var statistics = await _mediator.Send(query, cancellationToken);
        return Ok(statistics);
    }

    [HttpGet("me/watchlist/recommendations")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMyRecommendedMovies(
        [FromQuery] int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyRecommendedMoviesQuery(Limit: limit);
        var recommendations = await _mediator.Send(query, cancellationToken);
        return Ok(recommendations);
    }

    [HttpGet("me/watchlist/genre/{genre}")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyWatchlistByGenre(
        string genre, 
        CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistByGenreQuery(Genre: genre);
        var watchlist = await _mediator.Send(query, cancellationToken);
        return Ok(watchlist);
    }

    [HttpGet("me/watchlist/year-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyWatchlistByYearRange(
        [FromQuery] int startYear, 
        [FromQuery] int endYear,
        CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistByYearRangeQuery(StartYear: startYear, EndYear: endYear);
        var watchlist = await _mediator.Send(query, cancellationToken);
        return Ok(watchlist);
    }

    [HttpGet("me/watchlist/rating-range")]
    public async Task<ActionResult<IEnumerable<WatchlistItem>>> GetMyWatchlistByRatingRange(
        [FromQuery] double minRating, 
        [FromQuery] double maxRating,
        CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistByRatingRangeQuery(MinRating: minRating, MaxRating: maxRating);
        var watchlist = await _mediator.Send(query, cancellationToken);
        return Ok(watchlist);
    }

    [HttpPost("me/watchlist/add")]
    public async Task<ActionResult<WatchlistItem>> AddToMyWatchlist(
        [FromBody] AddToWatchlistDto dto, 
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new AddToWatchlistCommand(
            MovieId: dto.MovieId,
            Status: dto.Status,
            Notes: dto.Notes
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetMyWatchlistItemById), 
            new { watchlistItemId = result.Value!.Id }, result.Value);
    }

    [HttpGet("me/watchlist/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> GetMyWatchlistItemById(
        int watchlistItemId, 
        CancellationToken cancellationToken)
    {
        var query = new GetMyWatchlistItemByIdQuery(WatchlistItemId: watchlistItemId);
        var watchlistItem = await _mediator.Send(query, cancellationToken);
        if (watchlistItem == null)
            return NotFound();

        return Ok(watchlistItem);
    }

    [HttpPut("me/watchlist/item/{watchlistItemId}")]
    public async Task<ActionResult<WatchlistItem>> UpdateMyWatchlistItem(
        int watchlistItemId, 
        [FromBody] UpdateWatchlistItemDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var command = new UpdateWatchlistItemCommand(
            WatchlistItemId: watchlistItemId,
            Status: dto.Status,
            IsFavorite: dto.IsFavorite,
            UserRating: dto.UserRating,
            Notes: dto.Notes,
            WatchedDate: dto.WatchedDate
        );

        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("me/watchlist/item/{watchlistItemId}")]
    public async Task<ActionResult> RemoveFromMyWatchlist(
        int watchlistItemId, 
        CancellationToken cancellationToken)
    {
        var command = new RemoveFromWatchlistCommand(WatchlistItemId: watchlistItemId);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }
} 