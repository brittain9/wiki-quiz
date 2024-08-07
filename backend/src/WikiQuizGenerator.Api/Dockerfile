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

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src /src

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Create entry point script
RUN echo '#!/bin/bash\n\
cd /src/src/WikiQuizGenerator.Api\n\
dotnet ef database update\n\
if [ $? -ne 0 ]; then\n\
    echo "Migration failed. Continuing with application startup."\n\
fi\n\
cd /app\n\
dotnet WikiQuizGenerator.Api.dll' > /app/entrypoint.sh \
&& chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
