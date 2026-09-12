FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Pufzi.Api/Pufzi.Api.csproj", "Pufzi.Api/"]
COPY ["Pufzi.Services/Pufzi.Services.csproj", "Pufzi.Services/"]
COPY ["Pufzi.Data/Pufzi.Data.csproj", "Pufzi.Data/"]
COPY ["Pufzi.Contracts/Pufzi.Contracts.csproj", "Pufzi.Contracts/"]
COPY ["Pufzi.Infrastructure/Pufzi.Infrastructure.csproj", "Pufzi.Infrastructure/"]

RUN dotnet restore "Pufzi.Api/Pufzi.Api.csproj"

COPY . .

WORKDIR "/src/Pufzi.Api"

RUN dotnet publish "Pufzi.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Pufzi.Api.dll"]