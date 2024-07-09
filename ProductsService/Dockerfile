# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["./ProductsService.csproj", "ProductsService/"]
RUN dotnet restore "ProductsService/ProductsService.csproj"
WORKDIR "/src/ProductsService"
COPY . .
RUN dotnet build "ProductsService.csproj" -c Debug -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProductsService.csproj" -c Debug -o /app/publish /p:UseAppHost=false

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