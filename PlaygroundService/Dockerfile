﻿ARG PROJECT_NAME=PlaygroundService

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJECT_NAME
WORKDIR /src
COPY ["./${PROJECT_NAME}/${PROJECT_NAME}.csproj", "${PROJECT_NAME}/"]
COPY ["./ProdPlaceMongoDatabaseTools/ProdPlaceMongoDatabaseTools.csproj", "ProdPlaceMongoDatabaseTools/"]
RUN dotnet restore "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
COPY ./ProdPlaceMongoDatabaseTools ./ProdPlaceMongoDatabaseTools
WORKDIR "/src/${PROJECT_NAME}"
COPY ./${PROJECT_NAME} .
RUN dotnet build "${PROJECT_NAME}.csproj" -c Debug -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "${PROJECT_NAME}.csproj" -c Debug -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
ARG PROJECT_NAME
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src /src

ENTRYPOINT ["/bin/bash", "-c", "/app/entrypoint.sh $PROJECT_NAME"]