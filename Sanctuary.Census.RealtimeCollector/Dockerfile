FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sanctuary.Census.RealtimeCollector/Sanctuary.Census.RealtimeCollector.csproj", "Sanctuary.Census.RealtimeCollector/"]
COPY ["Sanctuary.Census.ServerData.Internal/Sanctuary.Census.ServerData.Internal.csproj", "Sanctuary.Census.ServerData.Internal/"]
COPY ["Sanctuary.Census.Common/Sanctuary.Census.Common.csproj", "Sanctuary.Census.Common/"]
RUN dotnet restore "Sanctuary.Census.RealtimeCollector/Sanctuary.Census.RealtimeCollector.csproj"
COPY . .
WORKDIR "/src/Sanctuary.Census.RealtimeCollector"
RUN dotnet build "Sanctuary.Census.RealtimeCollector.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sanctuary.Census.RealtimeCollector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sanctuary.Census.RealtimeCollector.dll"]
