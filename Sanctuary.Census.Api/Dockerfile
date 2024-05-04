FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sanctuary.Census.Api/Sanctuary.Census.Api.csproj", "Sanctuary.Census.Api/"]
COPY ["Sanctuary.Census.Common/Sanctuary.Census.Common.csproj", "Sanctuary.Census.Common/"]
RUN dotnet restore "Sanctuary.Census.Api/Sanctuary.Census.Api.csproj"
COPY . .
WORKDIR "/src/Sanctuary.Census.Api"
RUN dotnet build "Sanctuary.Census.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sanctuary.Census.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sanctuary.Census.Api.dll"]
