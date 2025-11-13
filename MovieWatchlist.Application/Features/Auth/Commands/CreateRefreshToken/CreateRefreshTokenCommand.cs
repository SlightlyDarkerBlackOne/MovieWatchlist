using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;

public record CreateRefreshTokenCommand(
    [Required(ErrorMessage = "User ID is required")]
    int UserId
) : IRequest<Result<RefreshTokenResult>>;

