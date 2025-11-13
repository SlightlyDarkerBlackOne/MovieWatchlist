using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Auth.Commands.ValidateToken;

public class ValidateTokenCommandHandler : IRequestHandler<ValidateTokenCommand, Result<ValidateTokenResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenExtractor _tokenExtractor;

    public ValidateTokenCommandHandler(
        IAuthenticationService authService,
        ITokenExtractor tokenExtractor)
    {
        _authService = authService;
        _tokenExtractor = tokenExtractor;
    }

    public async Task<Result<ValidateTokenResponse>> Handle(
        ValidateTokenCommand request,
        CancellationToken cancellationToken)
    {
        var token = _tokenExtractor.ExtractTokenFromHeader();
        if (string.IsNullOrEmpty(token))
        {
            return Result<ValidateTokenResponse>.Failure("Token not provided");
        }

        var isValid = await _authService.ValidateTokenAsync(token);
        var response = new ValidateTokenResponse(isValid);
        return Result<ValidateTokenResponse>.Success(response);
    }
}

