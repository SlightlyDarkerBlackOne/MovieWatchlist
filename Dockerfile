# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY MovieWatchlist.Api/*.csproj ./MovieWatchlist.Api/
COPY MovieWatchlist.Core/*.csproj ./MovieWatchlist.Core/
COPY MovieWatchlist.Application/*.csproj ./MovieWatchlist.Application/
COPY MovieWatchlist.Infrastructure/*.csproj ./MovieWatchlist.Infrastructure/
COPY MovieWatchlist.Persistence/*.csproj ./MovieWatchlist.Persistence/

# Restore dependencies for API project only
RUN dotnet restore MovieWatchlist.Api/MovieWatchlist.Api.csproj

# Copy all source code
COPY . .

# Build the application
RUN dotnet build MovieWatchlist.Api/MovieWatchlist.Api.csproj --configuration Release --no-restore

# Publish the application
RUN dotnet publish MovieWatchlist.Api/MovieWatchlist.Api.csproj --configuration Release --no-build --output /app/publish

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose the port
EXPOSE 10000

# Set the entry point
ENTRYPOINT ["dotnet", "MovieWatchlist.Api.dll"]
