using MovieWatchlist.Core.DTOs;

namespace MovieWatchlist.Core.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(RegisterDto registerDto);
    Task<AuthenticationResult> LoginAsync(LoginDto loginDto);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string token);
    Task<PasswordResetResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<PasswordResetResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
