using Microsoft.Extensions.Configuration;
using MovieWatchlist.Api.Constants;

namespace MovieWatchlist.Api.Helpers;

public static class ConfigurationHelper
{
    public static string GetConnectionString(
        this IConfiguration configuration,
        string connectionStringName = ConfigurationConstants.DEFAULT_CONNECTION_STRING)
    {
        return configuration[EnvironmentVariables.DATABASE_CONNECTION_STRING]
            ?? configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Database connection string '{connectionStringName}' is required");
    }

    public static string GetRequiredValue(
        this IConfiguration configuration,
        string environmentVariableKey,
        string configurationKey,
        string? defaultValue = null)
    {
        var value = configuration[environmentVariableKey]
            ?? configuration[configurationKey]
            ?? defaultValue;

        if (value == null)
        {
            throw new InvalidOperationException(
                $"Configuration value is required. Set either environment variable '{environmentVariableKey}' or configuration key '{configurationKey}'");
        }

        return value;
    }

    public static string GetOptionalValue(
        this IConfiguration configuration,
        string environmentVariableKey,
        string configurationKey,
        string defaultValue)
    {
        return configuration[environmentVariableKey]
            ?? configuration[configurationKey]
            ?? defaultValue;
    }

    public static int GetRequiredInt(
        this IConfiguration configuration,
        string environmentVariableKey,
        string configurationKey)
    {
        var value = configuration.GetRequiredValue(environmentVariableKey, configurationKey);
        
        if (!int.TryParse(value, out var intValue))
        {
            throw new InvalidOperationException(
                $"Configuration value must be a valid integer. Environment variable '{environmentVariableKey}' or configuration key '{configurationKey}'");
        }

        return intValue;
    }
}

