#!/bin/bash
set -e

echo "Starting entrypoint script"

# Run the application
echo "Starting the application..."
exec dotnet ProxyService.dll