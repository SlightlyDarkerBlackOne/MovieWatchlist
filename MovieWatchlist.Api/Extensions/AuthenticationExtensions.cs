using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Api.Helpers;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
                
                jwtSettings.SecretKey = configuration.GetOptionalValue(
                    EnvironmentVariables.JWT_SECRET_KEY,
                    ConfigurationConstants.JWT_SETTINGS_SECRET_KEY,
                    jwtSettings.SecretKey);
                jwtSettings.Issuer = configuration.GetOptionalValue(
                    EnvironmentVariables.JWT_ISSUER,
                    ConfigurationConstants.JWT_SETTINGS_ISSUER,
                    jwtSettings.Issuer);
                jwtSettings.Audience = configuration.GetOptionalValue(
                    EnvironmentVariables.JWT_AUDIENCE,
                    ConfigurationConstants.JWT_SETTINGS_AUDIENCE,
                    jwtSettings.Audience);
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Token))
                        {
                            var tokenFromCookie = context.Request.Cookies[CookieNames.AccessToken];
                            if (!string.IsNullOrEmpty(tokenFromCookie))
                            {
                                context.Token = tokenFromCookie;
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}

