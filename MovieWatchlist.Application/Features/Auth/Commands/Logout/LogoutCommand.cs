using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Logout;

public record LogoutCommand() : IRequest<Result<LogoutResponse>>;

