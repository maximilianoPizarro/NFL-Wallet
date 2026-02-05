#!/usr/bin/env bash
# Package the Helm chart and update the repo index in docs/ for GitHub Pages.
# Run from repo root: ./scripts/helm-package-docs.sh
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CHART_PATH="$REPO_ROOT/helm/nfl-wallet"
DOCS_PATH="$REPO_ROOT/docs"
# URL where the chart tarballs will be served (GitHub Pages)
REPO_URL="${HELM_REPO_URL:-https://maximilianopizarro.github.io/NFL-Wallet}"

cd "$REPO_ROOT"
helm package "$CHART_PATH" --destination "$DOCS_PATH"
helm repo index "$DOCS_PATH" --url "$REPO_URL" --merge "$DOCS_PATH/index.yaml"
echo "Done. Chart packaged in $DOCS_PATH. index.yaml updated."
echo "Add and commit docs/*.tgz and docs/index.yaml, then push."
