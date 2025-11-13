using System.ComponentModel.DataAnnotations;
using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email
) : IRequest<Result<PasswordResetResponse>>;

