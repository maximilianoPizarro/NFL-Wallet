#!/usr/bin/env bash
# Build all Stadium Wallet images and push to quay.io
# Usage: ./build-push-quay.sh [version] [quay-namespace]
#   version      : image tag (e.g. 1.0.1). If omitted, only :latest is pushed.
#   quay-namespace: Quay org/user (default: maximilianopizarro)
# Requires: podman, logged in to quay.io (podman login quay.io)

set -e
VERSION="${1:-}"
QUAY_NS="${2:-maximilianopizarro}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "Quay namespace : $QUAY_NS"
echo "Version tag    : ${VERSION:-<none, latest only>}"
echo ""

IMAGES=(
  "nfl-wallet-api-customers:ApiCustomers:ApiCustomers/Containerfile"
  "nfl-api-bills:ApiWalletBuffaloBills:ApiWalletBuffaloBills/Containerfile"
  "nfl-wallet-api-raiders:ApiWalletLasVegasRaiders:ApiWalletLasVegasRaiders/Containerfile"
  "nfl-wallet-webapp:frontend:frontend/Containerfile"
)

for entry in "${IMAGES[@]}"; do
  IFS=: read -r NAME CONTEXT CONTAINERFILE <<< "$entry"
  FULL="quay.io/${QUAY_NS}/${NAME}"

  echo "=== Building ${NAME} ==="
  TAG_ARGS="-t ${FULL}:latest"
  if [[ -n "$VERSION" ]]; then
    TAG_ARGS="${TAG_ARGS} -t ${FULL}:${VERSION}"
  fi

  podman build ${TAG_ARGS} -f "${CONTAINERFILE}" "${CONTEXT}"

  echo "=== Pushing ${NAME} ==="
  podman push "${FULL}:latest"
  if [[ -n "$VERSION" ]]; then
    podman push "${FULL}:${VERSION}"
  fi
  echo ""
done

echo "=== All images built and pushed to quay.io/${QUAY_NS} ==="
echo "  - nfl-wallet-api-customers:latest${VERSION:+ / $VERSION}"
echo "  - nfl-api-bills:latest${VERSION:+ / $VERSION}"
echo "  - nfl-wallet-api-raiders:latest${VERSION:+ / $VERSION}"
echo "  - nfl-wallet-webapp:latest${VERSION:+ / $VERSION}"
