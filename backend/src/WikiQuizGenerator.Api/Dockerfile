# Build stage (trigger backend workflow)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore as distinct layers for better caching
COPY WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj WikiQuizGenerator.Api/
COPY WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj WikiQuizGenerator.Core/
COPY WikiQuizGenerator.Data.Cosmos/WikiQuizGenerator.Data.Cosmos.csproj WikiQuizGenerator.Data.Cosmos/
COPY WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj WikiQuizGenerator.LLM/
RUN dotnet restore WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj

# Copy the full source and publish
COPY . .
WORKDIR /src/WikiQuizGenerator.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Listen on 8080 inside the container (good default for Azure Container Apps)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]


