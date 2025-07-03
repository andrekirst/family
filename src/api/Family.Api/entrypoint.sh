#!/bin/bash
set -e

echo "Starting Family API..."

# Wait for database to be ready
echo "Waiting for database to be ready..."
until dotnet ef database update --no-build --verbose; do
  echo "Database is unavailable - sleeping"
  sleep 1
done

echo "Database is ready - executing migrations..."
dotnet ef database update --no-build --verbose

echo "Starting application..."
exec dotnet Family.Api.dll