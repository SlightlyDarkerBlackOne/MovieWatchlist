using System.Security.Claims;
using Microsoft.Extensions.Options;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.DTOs;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public AuthenticationService(
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IRepository<PasswordResetToken> passwordResetTokenRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
    }

    public async Task<AuthenticationResult> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUserByEmail = await _userRepository.FindAsync(u => u.Email == registerDto.Email);
        if (existingUserByEmail.Any())
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Email is already registered"
            };
        }

        var existingUserByUsername = await _userRepository.FindAsync(u => u.Username == registerDto.Username);
        if (existingUserByUsername.Any())
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Username is already taken"
            };
        }

        // Create new user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new AuthenticationResult
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthenticationResult> LoginAsync(LoginDto loginDto)
    {
        // Find user by username or email
        var user = await FindUserByUsernameOrEmailAsync(loginDto.UsernameOrEmail);
        if (user == null)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid username/email or password"
            };
        }

        // Verify password
        if (!PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid username/email or password"
            };
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate tokens
        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new AuthenticationResult
        {
            IsSuccess = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        var principal = _jwtTokenService.ValidateToken(token);
        return Task.FromResult(principal != null);
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _refreshTokenRepository.FindAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);
        var token = tokenEntity.FirstOrDefault();

        if (token == null || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(token.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Revoke old refresh token
        token.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(token);

        // Generate new tokens
        var newToken = _jwtTokenService.GenerateToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return newToken;
    }

    public async Task<bool> LogoutAsync(string token)
    {
        try
        {
            var principal = _jwtTokenService.ValidateToken(token);
            if (principal == null) return false;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return false;

            // Revoke all refresh tokens for this user
            var userRefreshTokens = await _refreshTokenRepository.FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);
            foreach (var refreshToken in userRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PasswordResetResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            // Find user by email
            var users = await _userRepository.FindAsync(u => u.Email == forgotPasswordDto.Email);
            var user = users.FirstOrDefault();

            // Always return success to prevent email enumeration attacks
            var response = new PasswordResetResponseDto
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent."
            };

            if (user == null)
            {
                return response;
            }

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Invalidate any existing reset tokens for this user
            var existingTokens = await _passwordResetTokenRepository.FindAsync(t => t.UserId == user.Id && !t.IsUsed);
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                await _passwordResetTokenRepository.UpdateAsync(token);
            }

            // Create new reset token
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = resetToken,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _passwordResetTokenRepository.AddAsync(passwordResetToken);
            await _unitOfWork.SaveChangesAsync();

            // Send email
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.Username);

            return response;
        }
        catch (Exception)
        {
            return new PasswordResetResponseDto
            {
                Success = false,
                Message = "An error occurred while processing your request."
            };
        }
    }

    public async Task<PasswordResetResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            // Find valid reset token
            var tokens = await _passwordResetTokenRepository.FindAsync(t => 
                t.Token == resetPasswordDto.Token && 
                !t.IsUsed && 
                t.ExpiresAt > DateTime.UtcNow);

            var resetToken = tokens.FirstOrDefault();
            if (resetToken == null)
            {
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired reset token."
                };
            }

            // Find user
            var users = await _userRepository.FindAsync(u => u.Id == resetToken.UserId);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Hash new password
            var hashedPassword = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            user.PasswordHash = hashedPassword;

            // Mark token as used
            resetToken.IsUsed = true;

            await _userRepository.UpdateAsync(user);
            await _passwordResetTokenRepository.UpdateAsync(resetToken);
            await _unitOfWork.SaveChangesAsync();

            return new PasswordResetResponseDto
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }
        catch (Exception)
        {
            return new PasswordResetResponseDto
            {
                Success = false,
                Message = "An error occurred while resetting your password."
            };
        }
    }

    private async Task<User?> FindUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var users = await _userRepository.FindAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        return users.FirstOrDefault();
    }
}

