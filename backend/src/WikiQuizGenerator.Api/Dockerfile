# Dockerfile using .NET 9.0
# Assumes the build context is the 'backend' directory

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /source

# Install vsdbg for debugging capabilities
RUN apt-get update && apt-get install -y --no-install-recommends unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

# Copy only .csproj files to leverage Docker layer caching
COPY src/WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj ./WikiQuizGenerator.Api/
COPY src/WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj ./WikiQuizGenerator.Core/
COPY src/WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj ./WikiQuizGenerator.Data/
COPY src/WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj ./WikiQuizGenerator.LLM/

# Restore dependencies
RUN dotnet restore "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"

# Copy the rest of the application's source code
COPY src/ ./

# Build the application
RUN dotnet build "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 4026 

# Copy published artifacts and debugger
COPY --from=publish /app/publish .
COPY --from=build --chown=app:app /vsdbg /vsdbg

ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]