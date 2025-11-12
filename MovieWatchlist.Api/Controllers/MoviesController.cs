using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;
using MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;
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

        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();
        return Ok(response);
    }

    [HttpGet("{tmdbId}")]
    public async Task<ActionResult<Movie>> GetMovieDetails(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsQuery(tmdbId));

        var response = result.Value.Adapt<MovieDetailsDto>();
        return Ok(response);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetPopularMovies([FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetPopularMoviesQuery(page));

        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();
        return Ok(response);
    }

    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByGenre(string genre, [FromQuery] int page = 1)
    {
        var result = await _mediator.Send(new GetMoviesByGenreQuery(genre, page));

        var response = result.Value.Adapt<IEnumerable<MovieDetailsDto>>();
        return Ok(response);
    }

    [HttpGet("tmdb/{tmdbId}")]
    public async Task<ActionResult<MovieDetailsDto>> GetMovieDetailsByTmdbId(int tmdbId)
    {
        var result = await _mediator.Send(new GetMovieDetailsByTmdbIdQuery(tmdbId));

        var response = result.Value.Adapt<MovieDetailsDto>();
        return Ok(response);
    }
} 