#!/bin/bash
set -e

PROJECT_NAME="$1"
echo "Starting entrypoint script for $PROJECT_NAME"

# Navigate back to the app directory
cd /app

# Run the application
echo "Starting the application..."
exec dotnet "$PROJECT_NAME.dll"