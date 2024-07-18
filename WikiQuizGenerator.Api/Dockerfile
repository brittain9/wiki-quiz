FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj", "WikiQuizGenerator.Api/"]
COPY ["WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj", "WikiQuizGenerator.Core/"]
COPY ["WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj", "WikiQuizGenerator.Data/"]
COPY ["WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj", "WikiQuizGenerator.LLM/"]

# Set the fallback folder to a Linux path
ENV NUGET_PACKAGES=/root/.nuget/packages
RUN dotnet restore "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"

# Copy the rest of the code
COPY . .

WORKDIR "/src/WikiQuizGenerator.Api"
RUN dotnet build "WikiQuizGenerator.Api.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "WikiQuizGenerator.Api.csproj" -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install the debugger
RUN apt-get update \
    && apt-get install -y --no-install-recommends unzip curl \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "WikiQuizGenerator.Api.dll"]
