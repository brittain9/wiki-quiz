# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj", "WikiQuizGenerator.Api/"]
COPY ["WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj", "WikiQuizGenerator.Core/"]
COPY ["WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj", "WikiQuizGenerator.Data/"]
COPY ["WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj", "WikiQuizGenerator.LLM/"]
RUN dotnet restore "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"

# Copy the rest of the code
COPY . .

# Build the project
WORKDIR "/src/WikiQuizGenerator.Api"
RUN dotnet build "WikiQuizGenerator.Api.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "WikiQuizGenerator.Api.csproj" -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]