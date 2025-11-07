using Mapster;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Application.Commands;

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

        config.NewConfig<Application.Commands.UserInfo, DTOs.UserInfo>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<AuthenticationResult, RegisterResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, LoginResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<AuthenticationResult, RefreshTokenResponse>()
            .Map(dest => dest.User, src => src.User)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt ?? DateTime.UtcNow);

        config.NewConfig<Application.Commands.PasswordResetResponse, DTOs.PasswordResetResponse>()
            .Map(dest => dest.Success, src => src.Success)
            .Map(dest => dest.Message, src => src.Message);
    }
}

