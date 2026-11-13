# Multi-stage Dockerfile for Beat The Rocky
# Optimized for Render, Fly.io, Railway, and Container deployments

# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["BeatTheRocky.Api/BeatTheRocky.Api.csproj", "BeatTheRocky.Api/"]
RUN dotnet restore "BeatTheRocky.Api/BeatTheRocky.Api.csproj"

# Copy all source files and publish release
COPY . .
WORKDIR "/src/BeatTheRocky.Api"
RUN dotnet publish "BeatTheRocky.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BeatTheRocky.Api.dll"]
