ARG PROJECT_NAME=ProductsService

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJECT_NAME
WORKDIR /src
COPY ["./${PROJECT_NAME}/${PROJECT_NAME}.csproj", "${PROJECT_NAME}/"]
COPY ["./ProdPlaceDatabaseTools/ProdPlaceDatabaseTools.csproj", "ProdPlaceDatabaseTools/"]
RUN dotnet restore "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
COPY ./ProdPlaceDatabaseTools ./ProdPlaceDatabaseTools
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

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.6
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy entrypoint script and ensure it uses LF line endings
COPY ./${PROJECT_NAME}/entrypoint.sh /app/entrypoint.sh
RUN sed -i 's/\r$//' /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]