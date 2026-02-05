# NFL Wallet Helm Chart

Deploys the NFL Stadium Wallet stack: **webapp** (Vue frontend) and three APIs (**api-customers**, **api-bills**, **api-raiders**) with the following layout:

| Service        | Service Port | Pod (container) Port |
|----------------|--------------|------------------------|
| api-customers  | 8080         | 8080                  |
| api-bills      | 8081         | 8080                  |
| api-raiders    | 8082         | 8080                  |
| webapp         | 5173         | 8080                  |

## Prerequisites

- Images built and pushed to Quay (see repo root `build-push-quay.sh`)
- Kubernetes 1.19+
- Helm 3

## Install

```bash
# Add namespace if needed
kubectl create namespace nfl-wallet

# Install with default values (Quay namespace: maximilianopizarro)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet

# Override Quay namespace
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set imageNamespace=YOUR_QUAY_NS

# Set API URLs for the browser (required when exposing via Ingress/Route)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set webapp.apiCustomersUrl=https://api-customers.YOUR_DOMAIN/api \
  --set webapp.apiBillsUrl=https://api-bills.YOUR_DOMAIN/api \
  --set webapp.apiRaidersUrl=https://api-raiders.YOUR_DOMAIN/api
```

## Values

| Key | Description | Default |
|-----|-------------|---------|
| `imageNamespace` | Quay.io namespace for images | `maximilianopizarro` |
| `apiCustomers.image` / `tag` | api-customers image | `nfl-wallet-api-customers:latest` |
| `apiBills.image` / `tag` | api-bills image | `nfl-api-bills:latest` |
| `apiRaiders.image` / `tag` | api-raiders image | `nfl-wallet-api-raiders:latest` |
| `webapp.image` / `tag` | webapp image | `nfl-wallet-webapp:latest` |
| `webapp.apiCustomersUrl` | Base URL for Customers API (browser) | `""` |
| `webapp.apiBillsUrl` | Base URL for Bills API (browser) | `""` |
| `webapp.apiRaidersUrl` | Base URL for Raiders API (browser) | `""` |
| `*.persistence.enabled` | Use PVC for SQLite data | `true` |

## Uninstall

```bash
helm uninstall nfl-wallet -n nfl-wallet
```
