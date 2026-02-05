#!/usr/bin/env bash
# Build all NFL Wallet images and push to quay.io
# Usage: ./build-push-quay.sh [quay-namespace]
# Default namespace: maximilianopizarro
# Requires: podman, logged in to quay.io (podman login quay.io)

set -e
QUAY_NS="${1:-maximilianopizarro}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "Using Quay namespace: $QUAY_NS"

# api-customers -> nfl-wallet-api-customers
echo "=== Building api-customers (nfl-wallet-api-customers) ==="
podman build -t "quay.io/${QUAY_NS}/nfl-wallet-api-customers:latest" -f ApiCustomers/Containerfile ApiCustomers
podman push "quay.io/${QUAY_NS}/nfl-wallet-api-customers:latest"

# api-bills -> nfl-api-bills
echo "=== Building api-bills (nfl-api-bills) ==="
podman build -t "quay.io/${QUAY_NS}/nfl-api-bills:latest" -f ApiWalletBuffaloBills/Containerfile ApiWalletBuffaloBills
podman push "quay.io/${QUAY_NS}/nfl-api-bills:latest"

# api-raiders -> nfl-wallet-api-raiders
echo "=== Building api-raiders (nfl-wallet-api-raiders) ==="
podman build -t "quay.io/${QUAY_NS}/nfl-wallet-api-raiders:latest" -f ApiWalletLasVegasRaiders/Containerfile ApiWalletLasVegasRaiders
podman push "quay.io/${QUAY_NS}/nfl-wallet-api-raiders:latest"

# webapp -> nfl-wallet-webapp (API URLs overridden at runtime via config.json or Ingress)
echo "=== Building webapp (nfl-wallet-webapp) ==="
podman build -t "quay.io/${QUAY_NS}/nfl-wallet-webapp:latest" -f frontend/Containerfile frontend
podman push "quay.io/${QUAY_NS}/nfl-wallet-webapp:latest"

echo "=== All images built and pushed to quay.io/${QUAY_NS} ==="
echo "  - nfl-wallet-api-customers:latest"
echo "  - nfl-api-bills:latest"
echo "  - nfl-wallet-api-raiders:latest"
echo "  - nfl-wallet-webapp:latest"
