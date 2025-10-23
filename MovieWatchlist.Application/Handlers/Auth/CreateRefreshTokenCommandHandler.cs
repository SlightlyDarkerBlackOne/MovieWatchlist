using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

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

