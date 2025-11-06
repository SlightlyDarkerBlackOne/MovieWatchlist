using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Handlers.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IAuthenticationService _authService;

    public LogoutCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var success = await _authService.LogoutAsync(request.Token);
        return Result<bool>.Success(success);
    }
}
