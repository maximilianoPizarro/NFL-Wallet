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

# Install with default values (Quay namespace: maximilianopizarro).
# Only the webapp gets an OpenShift Route. The webapp proxies /api-customers, /api-bills, /api-raiders to the internal API services.
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet

# Disable the webapp Route
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set webapp.route.enabled=false
```

## Values

| Key | Description | Default |
|-----|-------------|---------|
| `imageNamespace` | Quay.io namespace for images | `maximilianopizarro` |
| `apiCustomers.image` / `tag` | api-customers image | `nfl-wallet-api-customers:latest` |
| `apiBills.image` / `tag` | api-bills image | `nfl-api-bills:latest` |
| `apiRaiders.image` / `tag` | api-raiders image | `nfl-wallet-api-raiders:latest` |
| `webapp.image` / `tag` | webapp image | `nfl-wallet-webapp:latest` |
| `webapp.route.enabled` | Create OpenShift Route for webapp | `true` |
| `webapp.route.host` | Override Route host | `""` |
| `webapp.route.tls.termination` | Route TLS: `edge`, `passthrough`, `reencrypt`; empty = no TLS | `edge` |
| `webapp.route.tls.insecureEdgeTerminationPolicy` | With edge: `Redirect` (HTTP→HTTPS), `Allow`, `None` | `Redirect` |
| `webapp.apiCustomersUrl` | Base URL for Customers API (browser, end with /api) | `""` |
| `webapp.apiBillsUrl` | Base URL for Bills API (browser, end with /api) | `""` |
| `webapp.apiRaidersUrl` | Base URL for Raiders API (optional) | `""` |
| `*.persistence.enabled` | Use PVC for SQLite data | `true` |
| `apiKeys.enabled` | Create Secret and inject API keys into APIs | `false` |
| `apiKeys.customers` / `bills` / `raiders` | API key per API (header `X-API-Key`) | `""` |
| `gateway.enabled` | Create Gateway API Gateway + HTTPRoutes | `false` |
| `gateway.className` | GatewayClass (e.g. `istio`, `openshift-gateway`) | `istio` |
| `gateway.existingGatewayName` | Use existing Gateway instead of creating one | `""` |
| `gateway.route.enabled` | OpenShift Route for the Gateway (Edge + Redirect) | `true` when gateway.enabled |
| `gateway.route.host` | Gateway Route host | `""` |
| `gateway.route.targetPort` | Gateway Service port (name or number) | `http` |
| `authorizationPolicy.enabled` | Istio AuthorizationPolicy: require `X-API-Key` header | `false` |
| `authorizationPolicy.requireForCustomers` / `requireForBills` / `requireForRaiders` | Apply policy only to these APIs (when enabled) | `true` each |
| `topology.applicationName` | Application name for OpenShift Topology grouping (`app.kubernetes.io/part-of`) | `nfl-wallet` |

With default values, the webapp Apache proxies `/api-customers`, `/api-bills`, and `/api-raiders` to the internal API services, so only the webapp Route is needed and the "Unexpected token '<'" error is avoided.

### Topology view (OpenShift)

Deployments and Services are labeled with `app.kubernetes.io/part-of` (default `nfl-wallet`) and `app.kubernetes.io/name` / `app.kubernetes.io/instance` so they group as one application in the Developer Topology view. The **webapp** deployment has the annotation `app.openshift.io/connects-to: ["api-customers","api-bills","api-raiders"]`, so the UI shows connectors from the frontend to the three APIs.

### Securing with API keys

To secure endpoints with one API key per API:

1. Set `apiKeys.enabled: true` and provide keys (e.g. `--set apiKeys.customers=secret1 --set apiKeys.bills=secret2 --set apiKeys.raiders=secret3`). Keys are stored in Secret `nfl-wallet-api-keys` and injected as env `ApiKey` into each API.
2. Each .NET API validates the `X-API-Key` header when `ApiKey` is set. The frontend sends these keys by default (hardcoded or via `config.json` `apiKeys`).
3. In Swagger UI for each API, use **Authorize** and set the same key to test.
4. Optionally enable `authorizationPolicy.enabled` when using Istio so the mesh requires the header before traffic reaches the pod.
5. Optionally enable `gateway.enabled` to expose APIs via Gateway API (HTTPRoute) instead of only ClusterIP services.

### Install with Connectivity Link (Gateway API)

To expose the APIs via Gateway API (HTTPRoute / Connectivity Link) instead of only Services:

1. **Cluster with Gateway API** (OpenShift includes Gateway API; if using Istio, ensure the GatewayClass is installed).

2. **Install with gateway enabled**:

```bash
# Create namespace
kubectl create namespace nfl-wallet

# Install with connectivity link (Gateway + HTTPRoutes for each API)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set gateway.enabled=true

# On OpenShift the platform GatewayClass is typically used (e.g. openshift-gateway)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.className=openshift-gateway

# To reuse an existing Gateway in the namespace
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.existingGatewayName=my-gateway
```

3. **What gets created**: a `Gateway` (if not using `existingGatewayName`), four `HTTPRoute`s—one for the **webapp** (path `/`) and three for the APIs (`/api-customers`, `/api-bills`, `/api-raiders`)—and an **OpenShift Route** `nfl-wallet-gateway` with TLS Edge + Redirect to expose the Gateway. Each API HTTPRoute uses a **URLRewrite** filter so that `/api-customers`, `/api-bills`, `/api-raiders` are rewritten to `/api` before reaching the backends (fixes 404 when calling e.g. `https://gateway-host/api-customers/Customers`). The frontend remains exposed via the webapp Route. You can disable the gateway Route with `--set gateway.route.enabled=false` if traffic is already entering through another path.

### Upgrade (gateway + API key auth for Raiders)

To update an existing release with the gateway path rewrite and **AuthorizationPolicy with API key only for the Raiders wallet API**:

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set authorizationPolicy.enabled=true \
  --set authorizationPolicy.requireForCustomers=false \
  --set authorizationPolicy.requireForBills=false \
  --set authorizationPolicy.requireForRaiders=true \
  --set apiKeys.enabled=true \
  --set apiKeys.raiders=nfl-wallet-raiders-key
```

Use the same key in the frontend (default in `client.js` is `nfl-wallet-raiders-key`) or set it in the webapp config. To require API key for all three APIs, omit the `requireFor*` overrides and set all three keys: `--set apiKeys.customers=... --set apiKeys.bills=... --set apiKeys.raiders=...`.

## Uninstall

```bash
helm uninstall nfl-wallet -n nfl-wallet
```
