#!/bin/bash
set -e

echo "Starting Family API..."

# Wait for database to be ready (simple connection test)
echo "Waiting for database to be ready..."
until timeout 1 bash -c "</dev/tcp/postgres-app/5432"; do
  echo "Database is unavailable - sleeping"
  sleep 1
done

echo "Database is ready!"

echo "Starting application..."
exec dotnet Family.Api.dll