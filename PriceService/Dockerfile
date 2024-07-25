# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["./PriceService/PriceService.csproj", "PriceService/"]
COPY ["./ProdPlaceDatabaseTools/ProdPlaceDatabaseTools.csproj", "ProdPlaceDatabaseTools/"]
RUN dotnet restore "PriceService/PriceService.csproj"
COPY ./ProdPlaceDatabaseTools ./ProdPlaceDatabaseTools
WORKDIR "/src/PriceService"
COPY ./PriceService .
RUN dotnet build "PriceService.csproj" -c Debug -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PriceService.csproj" -c Debug -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src /src

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.6
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy entrypoint script and ensure it uses LF line endings
COPY ./entrypoint.sh /app/entrypoint.sh
RUN sed -i 's/\r$//' /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]