using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenDto(
    [Required(ErrorMessage = "Refresh token is required")]
    string RefreshToken
);

