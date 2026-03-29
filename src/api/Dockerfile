# Use the official .NET 10.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Performance optimizations
ENV DOTNET_gcServer=true
ENV DOTNET_gcConcurrent=true
ENV DOTNET_PROCESSOR_COUNT=2
# Garbage Collection optimizations
ENV DOTNET_GarbageCollectionAdaptationMode=1
ENV DOTNET_ServerGarbageCollection=true
ENV DOTNET_ConcurrentGarbageCollection=true

# Expose port 80
EXPOSE 5000

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install required packages
RUN apk add --no-cache \
    curl \
    libgcc \
    libssl3 \
    ca-certificates-bundle \
    zlib \
    icu-libs \
    icu-data-full \
    libgdiplus

# Copy csproj and restore as distinct layers
COPY ["src/api/FamilyChat.Domain.Shared/FamilyChat.Domain.Shared.csproj", "src/api/FamilyChat.Domain.Shared/"]
COPY ["src/api/FamilyChat.Domain/FamilyChat.Domain.csproj", "src/api/FamilyChat.Domain/"]
COPY ["src/api/FamilyChat.Application.Contracts/FamilyChat.Application.Contracts.csproj", "src/api/FamilyChat.Application.Contracts/"]
COPY ["src/api/FamilyChat.Application/FamilyChat.Application.csproj", "src/api/FamilyChat.Application/"]
COPY ["src/api/FamilyChat.EntityFrameworkCore/FamilyChat.EntityFrameworkCore.csproj", "src/api/FamilyChat.EntityFrameworkCore/"]
COPY ["src/api/FamilyChat.HttpApi/FamilyChat.HttpApi.csproj", "src/api/FamilyChat.HttpApi/"]

RUN dotnet restore "src/api/FamilyChat.HttpApi/FamilyChat.HttpApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/api/FamilyChat.HttpApi"
RUN dotnet build "FamilyChat.HttpApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FamilyChat.HttpApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "FamilyChat.HttpApi.dll"]
