using Mapster;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;
using MovieWatchlist.Application.Features.Auth.Commands.Login;
using MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;
using MovieWatchlist.Application.Features.Auth.Commands.Register;
using MovieWatchlist.Application.Features.Auth.Commands.ResetPassword;
using MovieWatchlist.Application.Features.Auth.Queries.GetCurrentUser;

namespace MovieWatchlist.Api.Mapping;

public class AuthMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterDto, RegisterCommand>()
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password);

        config.NewConfig<LoginDto, LoginCommand>()
            .Map(dest => dest.UsernameOrEmail, src => src.UsernameOrEmail)
            .Map(dest => dest.Password, src => src.Password);

        config.NewConfig<ForgotPasswordDto, ForgotPasswordCommand>()
            .Map(dest => dest.Email, src => src.Email);

        config.NewConfig<ResetPasswordDto, ResetPasswordCommand>()
            .Map(dest => dest.Token, src => src.Token)
            .Map(dest => dest.NewPassword, src => src.NewPassword);

        config.NewConfig<AuthenticationResult, RegisterResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, LoginResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, RefreshTokenResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);
    }
}

