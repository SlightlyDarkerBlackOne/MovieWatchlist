using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyStatistics;

public record GetMyStatisticsQuery() : IRequest<WatchlistStatistics>;

