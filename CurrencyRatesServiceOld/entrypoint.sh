#!/bin/bash
set -e

echo "Starting entrypoint script"

# Navigate back to the app directory
cd /app

# Run the application
echo "Starting the application..."
exec dotnet CurrencyRatesService.dll