# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["api/src/core/Core.Library/Core.Library.csproj", "api/src/core/Core.Library/"]
COPY ["api/src/core/Core.Domain/Core.Domain.csproj", "api/src/core/Core.Domain/"]
COPY ["api/src/core/Core.Services/Core.Services.csproj", "api/src/core/Core.Services/"]
COPY ["api/src/infrastructures/Infra.Persistence/Infra.Persistence.csproj", "api/src/infrastructures/Infra.Persistence/"]
COPY ["api/src/applications/App.Processor/App.Processor.csproj", "api/src/applications/App.Processor/"]
COPY ["api/src/adapters/Adapter.Scheduler/Adapter.Scheduler.csproj", "api/src/adapters/Adapter.Scheduler/"]
RUN dotnet restore "./api/src/adapters/Adapter.Scheduler/Adapter.Scheduler.csproj"
COPY . .
WORKDIR "/src/api/src/adapters/Adapter.Scheduler"
RUN dotnet build "./Adapter.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Adapter.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adapter.Scheduler.dll"]
