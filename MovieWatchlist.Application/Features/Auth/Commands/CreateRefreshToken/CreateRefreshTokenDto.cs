using System.ComponentModel.DataAnnotations;

namespace MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;

public record CreateRefreshTokenDto(
    [Required(ErrorMessage = "User ID is required")]
    int UserId
);

