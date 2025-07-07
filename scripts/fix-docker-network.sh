#!/bin/bash
# Script to fix Docker network issues

echo "Fixing Docker network issues..."

# Stop all containers
echo "Stopping all containers..."
docker compose down

# Remove the problematic network
echo "Removing old network..."
docker network rm family_family-network 2>/dev/null || true

# Prune unused networks
echo "Pruning unused networks..."
docker network prune -f

# Optional: Reset Docker networking (requires sudo)
if [ "$1" == "--reset" ]; then
    echo "Resetting Docker daemon..."
    sudo systemctl restart docker
    sleep 5
fi

# Create network with explicit configuration
echo "Creating new network with stable configuration..."
docker network create \
    --driver bridge \
    --subnet=172.20.0.0/16 \
    --gateway=172.20.0.1 \
    --opt com.docker.network.bridge.enable_icc=true \
    --opt com.docker.network.bridge.enable_ip_masquerade=true \
    family_family-network

# Start containers again
echo "Starting containers..."
docker compose up -d

echo "Done! Network should be stable now."