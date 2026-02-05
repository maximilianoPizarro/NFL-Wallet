---
layout: default
title: Deployment
description: "Deploy NFL Stadium Wallet with the Helm chart to Kubernetes or OpenShift."
---

## Deploying With the Helm Chart

The solution can be deployed to Kubernetes or OpenShift using the provided **Helm chart** under `helm/nfl-wallet`. The chart deploys:

- **webapp** — Vue frontend (with optional OpenShift Route, TLS edge + redirect).
- **api-customers** — Customers API.
- **api-bills** — Buffalo Bills wallet API.
- **api-raiders** — Las Vegas Raiders wallet API.

Each API has a Service, optional PVC for SQLite, and optional API key injection from a Secret. The webapp proxies `/api-customers`, `/api-bills`, and `/api-raiders` to the internal API services, so only the webapp Route is needed for external access by default.

### Prerequisites

- Images built and pushed to a registry (e.g. Quay). See the repo root `build-push-quay.sh` if applicable.
- Kubernetes 1.19+ or OpenShift.
- Helm 3.

### Install (default)

```bash
kubectl create namespace nfl-wallet

helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet
```

Default values use the Quay namespace and image names defined in `values.yaml`. Only the webapp gets an OpenShift Route by default.

### Override image registry / namespace

```bash
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set imageNamespace=your-quay-namespace
```

### Disable the webapp Route

```bash
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set webapp.route.enabled=false
```

### Helm chart values (reference)

| Key | Description | Default |
|-----|-------------|---------|
| `global.imageRegistry` | Image registry | `quay.io` |
| `global.imagePullSecrets` | Pull secrets | `[]` |
| `imageNamespace` | Registry namespace for images | `maximilianopizarro` |
| `topology.applicationName` | OpenShift Topology `app.kubernetes.io/part-of` | `nfl-wallet` |
| `apiCustomers.image`, `tag` | api-customers image | `nfl-wallet-api-customers:latest` |
| `apiCustomers.replicaCount` | Replicas | `1` |
| `apiCustomers.service.port` | Service port | `8080` |
| `apiCustomers.persistence.enabled`, `size` | PVC for SQLite | `true`, `1Gi` |
| `apiBills.image`, `tag` | api-bills image | `nfl-api-bills:latest` |
| `apiBills.replicaCount` | Replicas | `1` |
| `apiBills.service.port` | Service port | `8081` |
| `apiBills.persistence.enabled`, `size` | PVC for SQLite | `true`, `1Gi` |
| `apiBills.rateLimit.permitLimit` | Rate limit: max requests per window (per IP) for /api-bills | `100` |
| `apiBills.rateLimit.windowSeconds` | Rate limit window (seconds) | `60` |
| `apiRaiders.image`, `tag` | api-raiders image | `nfl-wallet-api-raiders:latest` |
| `apiRaiders.replicaCount` | Replicas | `1` |
| `apiRaiders.service.port` | Service port | `8082` |
| `apiRaiders.persistence.enabled`, `size` | PVC for SQLite | `true`, `1Gi` |
| `webapp.image`, `tag` | webapp image | `nfl-wallet-webapp:latest` |
| `webapp.replicaCount` | Replicas | `1` |
| `webapp.service.port` | Service port | `5173` |
| `webapp.route.enabled` | Create OpenShift Route for webapp | `true` |
| `webapp.route.host` | Override Route host | `""` |
| `webapp.route.tls.termination` | Route TLS (edge, passthrough, reencrypt) | `edge` |
| `webapp.route.tls.insecureEdgeTerminationPolicy` | HTTP→HTTPS redirect | `Redirect` |
| `webapp.apiCustomersUrl`, `apiBillsUrl`, `apiRaidersUrl` | Override API base URLs (browser) | `""` |
| `apiKeys.enabled` | Create Secret and inject API keys | `false` |
| `apiKeys.customers`, `bills`, `raiders` | API key per API (X-API-Key) | `""` |
| `gateway.enabled` | Create Gateway + HTTPRoutes | `false` |
| `gateway.className` | GatewayClass (e.g. istio, openshift-gateway) | `istio` |
| `gateway.existingGatewayName` | Use existing Gateway | `""` |
| `gateway.route.enabled` | OpenShift Route for Gateway (edge + redirect) | `true` when gateway enabled |
| `gateway.route.host`, `targetPort` | Gateway Route host and port | `""`, `http` |
| `authorizationPolicy.enabled` | Istio AuthorizationPolicy for X-API-Key | `false` |
| `authorizationPolicy.requireForCustomers`, `requireForBills`, `requireForRaiders` | Apply policy per API | `true` each |

### Generate Helm package and index.yaml (docs folder)

To publish the chart as a Helm repo from the **docs** folder (e.g. for GitHub Pages):

1. From the **repo root** run:
   - **Linux/macOS:** `./scripts/helm-package-docs.sh`
   - **Windows (PowerShell):** `.\scripts\helm-package-docs.ps1`
2. This creates `docs/nfl-wallet-<version>.tgz` and updates (or creates) `docs/index.yaml`.
3. Commit and push `docs/nfl-wallet-*.tgz` and `docs/index.yaml`.
4. Users can then add the repo and install:
   ```bash
   helm repo add nfl-wallet https://maximilianopizarro.github.io/NFL-Wallet
   helm repo update
   helm install nfl-wallet nfl-wallet/nfl-wallet -n nfl-wallet
   ```

For more details and security/topology options, see the [Helm chart README](https://github.com/maximilianopizarro/NFL-Wallet/blob/main/helm/nfl-wallet/README.md) in the repo.

### OpenShift Topology view

When deployed to OpenShift, the chart adds labels and the `app.openshift.io/connects-to` annotation so the Developer Topology view groups the app and shows connectors from the webapp to the three APIs.

[![Topology view]({{ '/topology.png' | relative_url }})]({{ '/topology.png' | relative_url }}){: .doc-img-link}
*NFL Wallet in OpenShift Topology (webapp → api-customers, api-bills, api-raiders). Click to enlarge.*
