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

### Main chart values (summary)

| Key | Description | Default |
|-----|-------------|---------|
| `imageNamespace` | Registry namespace for images | (see values.yaml) |
| `webapp.route.enabled` | Create OpenShift Route for webapp | `true` |
| `webapp.route.tls.termination` | Route TLS (edge, passthrough, reencrypt) | `edge` |
| `webapp.route.tls.insecureEdgeTerminationPolicy` | HTTP→HTTPS redirect | `Redirect` |
| `apiKeys.enabled` | Create Secret and inject API keys into APIs | `false` |
| `gateway.enabled` | Create Gateway API Gateway + HTTPRoutes | `false` |
| `authorizationPolicy.enabled` | Istio AuthorizationPolicy for X-API-Key | `false` |

For the full list and security/topology options, see the [Helm chart README](https://github.com/maximilianopizarro/NFL-Wallet/blob/main/helm/nfl-wallet/README.md) in the repo.

### OpenShift Topology view

When deployed to OpenShift, the chart adds labels and the `app.openshift.io/connects-to` annotation so the Developer Topology view groups the app and shows connectors from the webapp to the three APIs.

![Topology view]({{ '/topology.png' | relative_url }}){: .doc-img}
*NFL Wallet in OpenShift Topology (webapp → api-customers, api-bills, api-raiders).*
