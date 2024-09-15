#!/bin/bash
set -e

PROJECT_NAME="$1"
echo "Starting entrypoint script for $PROJECT_NAME"

# Run the application
echo "Starting the application..."
exec dotnet "$PROJECT_NAME.dll"