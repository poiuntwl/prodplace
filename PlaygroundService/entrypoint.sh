#!/bin/bash
set -e

echo "Starting entrypoint script"
SERVICE_NAME="$1"

# Run the application
echo "Starting the application: $SERVICE_NAME"
exec dotnet "$SERVICE_NAME.dll"
