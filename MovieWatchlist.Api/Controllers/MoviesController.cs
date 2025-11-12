using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Movie>>> SearchMovies([FromQuery] string query, [FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new SearchMoviesQuery(query, page));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();
        return Ok(response);
    }

    [HttpGet("{tmdbId}")]
    public async Task<ActionResult<Movie>> GetMovieDetails(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsQuery(tmdbId));
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        var response = result.Value.Adapt<MovieDetailsDto>();
        return Ok(response);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetPopularMovies([FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetPopularMoviesQuery(page));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
            
        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();

        return Ok(response);
    }

    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByGenre(string genre, [FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetMoviesByGenreQuery(genre, page));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();
        return Ok(response);
    }

    [HttpGet("tmdb/{tmdbId}")]
    public async Task<ActionResult<MovieDetailsDto>> GetMovieDetailsByTmdbId(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsByTmdbIdQuery(tmdbId));
        if (result.IsFailure)
        {
            if (result.Error.Equals(ErrorMessages.TmdbRateLimitExceeded, StringComparison.Ordinal))
                return StatusCode(StatusCodes.Status429TooManyRequests, new { message = result.Error });

            if (result.Error.StartsWith(ErrorMessages.FailedToFetchMovieData, StringComparison.Ordinal))
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.Error });

            if (result.Error.StartsWith("Movie with TMDB ID", StringComparison.Ordinal))
                return NotFound(new { message = result.Error });

            return BadRequest(new { error = result.Error });
        }

        var response = result.Value.Adapt<MovieDetailsDto>();
        return Ok(response);
    }
} 