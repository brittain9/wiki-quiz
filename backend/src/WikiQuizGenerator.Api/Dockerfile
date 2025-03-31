# Dockerfile using .NET 9.0

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

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
# Expose HTTP port
EXPOSE 80

# Copy only the published artifacts from the 'publish' stage
COPY --from=publish /app/publish .

# Set the entrypoint to run the application DLL
ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]