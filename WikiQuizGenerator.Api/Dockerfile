FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY ["WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj", "WikiQuizGenerator.Api/"]
COPY ["WikiQuizGenerator.Core/WikiQuizGenerator.Core.csproj", "WikiQuizGenerator.Core/"]
COPY ["WikiQuizGenerator.Data/WikiQuizGenerator.Data.csproj", "WikiQuizGenerator.Data/"]
COPY ["WikiQuizGenerator.LLM/WikiQuizGenerator.LLM.csproj", "WikiQuizGenerator.LLM/"]
RUN dotnet restore "WikiQuizGenerator.Api/WikiQuizGenerator.Api.csproj"

# Copy the rest of the code
COPY . .

# Install the debugger
RUN apt-get update \
    && apt-get install -y --no-install-recommends unzip \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

WORKDIR "/app/WikiQuizGenerator.Api"
ENTRYPOINT ["dotnet", "run", "--no-launch-profile", "--no-restore", "--urls", "http://+:80"]
