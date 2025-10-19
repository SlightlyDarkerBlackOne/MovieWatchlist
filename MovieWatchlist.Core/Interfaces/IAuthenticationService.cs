using MovieWatchlist.Core.Commands;

namespace MovieWatchlist.Core.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(RegisterCommand command);
    Task<AuthenticationResult> LoginAsync(LoginCommand command);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string token);
    Task<PasswordResetResponse> ForgotPasswordAsync(ForgotPasswordCommand command);
    Task<PasswordResetResponse> ResetPasswordAsync(ResetPasswordCommand command);
}
