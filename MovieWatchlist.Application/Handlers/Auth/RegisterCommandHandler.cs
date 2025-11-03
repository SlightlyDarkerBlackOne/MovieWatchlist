using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Handlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing registration for user: {Username}", request.Username);

        var userResult = await _authService.RegisterUserAsync(request);
        
        if (userResult.IsFailure)
            return Result<AuthenticationResult>.Failure(userResult.Error);

        await _unitOfWork.SaveChangesAsync();

        var authResult = await _authService.GenerateAuthenticationResultWithRefreshTokenAsync(userResult.Value!);
        
        _logger.LogInformation("User registration completed successfully for: {Username}", request.Username);
        
        return Result<AuthenticationResult>.Success(authResult);
    }
}

