using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthCookieService _cookieService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authService,
        IUnitOfWork unitOfWork,
        IAuthCookieService cookieService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _cookieService = cookieService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var userResult = await _authService.RegisterUserAsync(request);
        
        if (userResult.IsFailure)
            return Result<RegisterResponse>.Failure(userResult.Error);

        await _unitOfWork.SaveChangesAsync();

        var authResult = await _authService.GenerateAuthenticationResultWithRefreshTokenAsync(userResult.Value!);
        
        _cookieService.SetAuthCookies(authResult.Token, authResult.RefreshToken, authResult.ExpiresAt);
        
        _logger.LogInformation("User registered successfully: {Username}", request.Username);
        
        var response = authResult.Adapt<RegisterResponse>();
        return Result<RegisterResponse>.Success(response);
    }
}

