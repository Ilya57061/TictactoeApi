FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TicTacToeArena/TicTacToeArena.csproj", "TicTacToeArena/"]
COPY ["TicTacToeArena.Application/TicTacToeArena.Application.csproj", "TicTacToeArena.Application/"]
COPY ["TicTacToeArena.Domain/TicTacToeArena.Domain.csproj", "TicTacToeArena.Domain/"]
COPY ["TicTacToeArena.Infrastructure/TicTacToeArena.Infrastructure.csproj", "TicTacToeArena.Infrastructure/"]
COPY ["TicTacToeArena.Persistence/TicTacToeArena.Persistence.csproj", "TicTacToeArena.Persistence/"]

RUN dotnet restore "./TicTacToeArena/TicTacToeArena.csproj"
COPY . .
WORKDIR "/src/TicTacToeArena"
RUN dotnet publish "./TicTacToeArena.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TicTacToeArena.dll"]
