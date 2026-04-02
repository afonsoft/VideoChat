# API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/afonsoft.FamilyMeet.HttpApi.Host/afonsoft.FamilyMeet.HttpApi.Host.csproj", "src/afonsoft.FamilyMeet.HttpApi.Host/"]
COPY ["src/afonsoft.FamilyMeet.HttpApi/afonsoft.FamilyMeet.HttpApi.csproj", "src/afonsoft.FamilyMeet.HttpApi/"]
COPY ["src/afonsoft.FamilyMeet.Application/afonsoft.FamilyMeet.Application.csproj", "src/afonsoft.FamilyMeet.Application/"]
COPY ["src/afonsoft.FamilyMeet.Application.Contracts/afonsoft.FamilyMeet.Application.Contracts.csproj", "src/afonsoft.FamilyMeet.Application.Contracts/"]
COPY ["src/afonsoft.FamilyMeet.Domain/afonsoft.FamilyMeet.Domain.csproj", "src/afonsoft.FamilyMeet.Domain/"]
COPY ["src/afonsoft.FamilyMeet.Domain.Shared/afonsoft.FamilyMeet.Domain.Shared.csproj", "src/afonsoft.FamilyMeet.Domain.Shared/"]
COPY ["src/afonsoft.FamilyMeet.EntityFrameworkCore/afonsoft.FamilyMeet.EntityFrameworkCore.csproj", "src/afonsoft.FamilyMeet.EntityFrameworkCore/"]

RUN dotnet restore "src/afonsoft.FamilyMeet.HttpApi.Host/afonsoft.FamilyMeet.HttpApi.Host.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/afonsoft.FamilyMeet.HttpApi.Host"
RUN dotnet build "afonsoft.FamilyMeet.HttpApi.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "afonsoft.FamilyMeet.HttpApi.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "afonsoft.FamilyMeet.HttpApi.Host.dll"]
