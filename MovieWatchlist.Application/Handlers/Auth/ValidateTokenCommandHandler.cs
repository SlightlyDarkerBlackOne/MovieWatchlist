using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Handlers.Auth;

public class ValidateTokenCommandHandler : IRequestHandler<ValidateTokenCommand, Result<bool>>
{
    private readonly IAuthenticationService _authService;

    public ValidateTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<bool>> Handle(
        ValidateTokenCommand request,
        CancellationToken cancellationToken)
    {
        var isValid = await _authService.ValidateTokenAsync(request.Token);
        return Result<bool>.Success(isValid);
    }
}
