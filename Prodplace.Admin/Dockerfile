ARG PROJECT_NAME

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT_NAME
WORKDIR /src
COPY ["./${PROJECT_NAME}/${PROJECT_NAME}.csproj", "${PROJECT_NAME}/"]
RUN dotnet restore "${PROJECT_NAME}/${PROJECT_NAME}.csproj"
COPY ./ProdPlace.Grpc/ ./ProdPlace.Grpc/
WORKDIR "/src/${PROJECT_NAME}"
COPY ./${PROJECT_NAME} .
RUN dotnet build "${PROJECT_NAME}.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "${PROJECT_NAME}.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Prodplace.Admin.dll"]