using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IAuthenticationService authService,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var userResult = await _authService.RegisterUserAsync(request);
        
        if (userResult.IsFailure)
            return Result<AuthenticationResult>.Failure(userResult.Error);

        await _unitOfWork.SaveChangesAsync();

        var authResult = await _authService.GenerateAuthenticationResultWithRefreshTokenAsync(userResult.Value!);
        
        return Result<AuthenticationResult>.Success(authResult);
    }
}

