# Dockerfile using .NET 9.0

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Allow overriding the build configuration (defaults to Release)
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install vsdbg for debugging capabilities
RUN apt-get update && apt-get install -y --no-install-recommends unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

# Copy only *.csproj files first and restore dependencies.
# This layer is cached efficiently unless project references change.
COPY ["src/WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj", "src/WikiQuizGenerator.Api/"]
COPY ["src/WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj", "src/WikiQuizGenerator.Core/"]
COPY ["src/WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj", "src/WikiQuizGenerator.Data/"]
COPY ["src/WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj", "src/WikiQuizGenerator.LLM/"]
# Adjust paths if your structure is different
RUN dotnet restore "./src/WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"

# Copy the rest of the source code
# Consider copying only necessary project directories instead of '.' for better caching
COPY src/WikiQuizGenerator.Api/ ./src/WikiQuizGenerator.Api/
COPY src/WikiQuizGenerator.Core/ ./src/WikiQuizGenerator.Core/
COPY src/WikiQuizGenerator.Data/ ./src/WikiQuizGenerator.Data/
COPY src/WikiQuizGenerator.LLM/ ./src/WikiQuizGenerator.LLM/

# Verify this WORKDIR path is correct relative to the initial /src WORKDIR
WORKDIR "/src/src/WikiQuizGenerator.Api"
RUN dotnet build "./WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Stage 2: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Ensure this WORKDIR context is correct for publish
WORKDIR "/src/src/WikiQuizGenerator.Api"
RUN dotnet publish "./WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

# Stage 3: Final runtime image
# Use the ASP.NET runtime image, matching the SDK version used for build (.NET 9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
# Run as non-root user
USER app
WORKDIR /app
# Expose HTTP port and Debugger Port
EXPOSE 80
# Debugger Port
EXPOSE 4026 

# Copy only the published artifacts from the 'publish' stage
COPY --from=publish /app/publish .
# Copy the debugger from the build stage
COPY --from=build --chown=app:app /vsdbg /vsdbg

# Set the entrypoint to run the application DLL
# The entrypoint needs to keep the container running, the debugger attaches later.
ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]