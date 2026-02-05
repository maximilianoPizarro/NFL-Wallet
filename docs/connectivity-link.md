---
layout: default
title: Connectivity Link (Gateway API)
description: "Enable the Gateway API and HTTPRoutes (Connectivity Link) for NFL Stadium Wallet with Helm."
---

## Adding the Connectivity Link (Gateway Layer)

You can expose the webapp and the three APIs through the **Gateway API** (Connectivity Link) by enabling the gateway in the Helm chart. The chart then creates:

- A **Gateway** (if you do not use an existing one).
- Four **HTTPRoutes**: one for the webapp (path `/`) and three for the APIs (`/api-customers`, `/api-bills`, `/api-raiders`), with URL rewrite so the backends receive `/api/...`.
- An **OpenShift Route** for the Gateway with TLS edge and redirect (optional, configurable).

### Commands to Enable the Gateway

**Install with gateway enabled (new install):**

```bash
kubectl create namespace nfl-wallet

helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set gateway.enabled=true
```

**On OpenShift**, use the platform GatewayClass (e.g. `openshift-gateway`):

```bash
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.className=openshift-gateway
```

**Reuse an existing Gateway** in the same namespace:

```bash
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.existingGatewayName=my-gateway
```

**Upgrade an existing release** to add the gateway:

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set gateway.enabled=true
```

### Gateway-related values

| Value | Description | Default |
|-------|-------------|---------|
| `gateway.enabled` | Create Gateway + HTTPRoutes | `false` |
| `gateway.className` | GatewayClass name (e.g. `istio`, `openshift-gateway`) | `istio` |
| `gateway.existingGatewayName` | Use existing Gateway instead of creating one | `""` |
| `gateway.route.enabled` | Create OpenShift Route for the Gateway (edge + redirect) | `true` when gateway enabled |
| `gateway.route.targetPort` | Target port of the Gateway Service | `http` |

### What gets created

- **Gateway** (if `gateway.existingGatewayName` is not set): one Gateway resource.
- **HTTPRoutes**: `nfl-wallet-webapp` (path `/`), `nfl-wallet-api-customers`, `nfl-wallet-api-bills`, `nfl-wallet-api-raiders` with path prefix and URL rewrite to `/api`.
- **OpenShift Route** `nfl-wallet-gateway`: exposes the Gateway with TLS edge and HTTP→HTTPS redirect (when `gateway.route.enabled` is true).

The frontend remains available via the webapp Route as before; the gateway Route provides an alternate entry point for the same backends.

### Connectivity Link overview

![Connectivity Link]({{ '/connectivity link.png' | relative_url | replace: ' ', '%20' }}){: .doc-img}
*Gateway API and HTTPRoutes (Connectivity Link) exposing the webapp and APIs.*
