using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Handlers.Auth;

public class CreateRefreshTokenCommandHandler : IRequestHandler<CreateRefreshTokenCommand, Result<RefreshTokenResult>>
{
    private readonly IAuthenticationService _authService;

    public CreateRefreshTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<RefreshTokenResult>> Handle(
        CreateRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return await _authService.CreateRefreshTokenAsync(request.UserId);
    }
}

