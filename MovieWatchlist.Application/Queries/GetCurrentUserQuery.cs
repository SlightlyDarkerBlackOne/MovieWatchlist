using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Queries;

public record GetCurrentUserQuery() : IRequest<Result<UserInfo>>;

