# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/api/Family.Api/Family.Api.csproj", "src/api/Family.Api/"]
RUN dotnet restore "src/api/Family.Api/Family.Api.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/api/Family.Api"

# Build application
RUN dotnet build "Family.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Family.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Entity Framework tool for migrations
USER root
RUN apt-get update && apt-get install -y curl
USER app

EXPOSE 8081

# Create entrypoint script to run migrations and start app
COPY src/api/Family.Api/entrypoint.sh /entrypoint.sh
USER root
RUN chmod +x /entrypoint.sh
USER app

ENTRYPOINT ["/entrypoint.sh"]