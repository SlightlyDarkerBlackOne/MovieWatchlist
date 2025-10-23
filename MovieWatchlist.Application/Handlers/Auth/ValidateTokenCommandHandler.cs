using MediatR;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

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
