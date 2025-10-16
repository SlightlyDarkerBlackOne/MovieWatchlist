# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY MovieWatchlist.Api/*.csproj ./MovieWatchlist.Api/
COPY MovieWatchlist.Core/*.csproj ./MovieWatchlist.Core/
COPY MovieWatchlist.Infrastructure/*.csproj ./MovieWatchlist.Infrastructure/
COPY MovieWatchlist.Tests/*.csproj ./MovieWatchlist.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
RUN dotnet build --configuration Release --no-restore

# Publish the application
RUN dotnet publish MovieWatchlist.Api --configuration Release --no-build --output /app/publish

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port
EXPOSE 10000

# Set the entry point
ENTRYPOINT ["dotnet", "MovieWatchlist.Api.dll"]
