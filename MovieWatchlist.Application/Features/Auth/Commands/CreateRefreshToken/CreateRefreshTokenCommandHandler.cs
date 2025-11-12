using MediatR;
using MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;

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

