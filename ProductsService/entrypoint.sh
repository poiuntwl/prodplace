#!/bin/bash
set -e

echo "Starting entrypoint script"

# Navigate to the directory containing the .csproj file
cd /src/ProductsService

# Wait for the database to be ready
echo "Waiting for database to be ready..."
until dotnet ef database update --verbose; do
    echo "Database is not ready - waiting..."
    sleep 5
done

echo "Database is ready, migrations have been applied successfully"

# Navigate back to the app directory
cd /app

# Run the application
echo "Starting the application..."
exec dotnet ProductsService.dll