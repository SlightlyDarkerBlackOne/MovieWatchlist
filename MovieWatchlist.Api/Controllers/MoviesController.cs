using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;
using MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;

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
    public async Task<ActionResult<IEnumerable<MovieDetailsDto>>> SearchMovies([FromQuery] string query, [FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new SearchMoviesQuery(query, page));

        return Ok(result.Value);
    }

    [HttpGet("{tmdbId}")]
    public async Task<ActionResult<MovieDetailsDto>> GetMovieDetails(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsQuery(tmdbId));

        return Ok(result.Value);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<MovieDetailsDto>>> GetPopularMovies([FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetPopularMoviesQuery(page));

        return Ok(result.Value);
    }

    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<IEnumerable<MovieDetailsDto>>> GetMoviesByGenre(string genre, [FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetMoviesByGenreQuery(genre, page));

        return Ok(result.Value);
    }

    [HttpGet("tmdb/{tmdbId}")]
    public async Task<ActionResult<MovieDetailsDto>> GetMovieDetailsByTmdbId(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsByTmdbIdQuery(tmdbId));

        return Ok(result.Value);
    }
} 