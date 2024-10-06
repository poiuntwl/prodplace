#!/bin/bash
set -e

PROJECT_NAME="$1"
echo "Starting entrypoint script for $PROJECT_NAME"

# Navigate to the directory containing the .csproj file
cd /src/$PROJECT_NAME

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
exec dotnet "$PROJECT_NAME.dll"