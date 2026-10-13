# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api/src/core/Core.Library/Core.Library.csproj", "api/src/core/Core.Library/"]
COPY ["api/src/core/Core.Domain/Core.Domain.csproj", "api/src/core/Core.Domain/"]
COPY ["api/src/core/Core.Services/Core.Services.csproj", "api/src/core/Core.Services/"]
COPY ["api/src/infrastructures/Infra.Persistence/Infra.Persistence.csproj", "api/src/infrastructures/Infra.Persistence/"]
COPY ["api/src/infrastructures/Infra.Caching/Infra.Caching.csproj", "api/src/infrastructures/Infra.Caching/"]
COPY ["api/src/applications/App.UseCases/App.UseCases.csproj", "api/src/applications/App.UseCases/"]
COPY ["api/src/adapters/Adapter.Restful/Adapter.Restful.csproj", "api/src/adapters/Adapter.Restful/"]
RUN dotnet restore "./api/src/adapters/Adapter.Restful/Adapter.Restful.csproj"
COPY . .
WORKDIR "/src/api/src/adapters/Adapter.Restful"
RUN dotnet build "./Adapter.Restful.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Adapter.Restful.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adapter.Restful.dll"]