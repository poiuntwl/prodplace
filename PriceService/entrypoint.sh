#!/bin/bash
set -e

echo "Starting entrypoint script"
SERVICE_NAME="$1"

# Navigate back to the app directory
cd /app

# Run the application
echo "Starting the application..."
exec dotnet PriceService.dll