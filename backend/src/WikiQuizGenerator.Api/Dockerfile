#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj", "src/WikiQuizGenerator.Api/"]
COPY ["src/WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj", "src/WikiQuizGenerator.Core/"]
COPY ["src/WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj", "src/WikiQuizGenerator.Data/"]
COPY ["src/WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj", "src/WikiQuizGenerator.LLM/"]
RUN dotnet restore "./src/WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"
COPY . .
WORKDIR "/src/src/WikiQuizGenerator.Api"
RUN dotnet build "./WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WikiQuizGenerator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]