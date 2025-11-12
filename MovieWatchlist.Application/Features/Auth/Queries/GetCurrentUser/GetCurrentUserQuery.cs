using MediatR;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery() : IRequest<Result<UserInfo>>;

