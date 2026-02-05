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
| `webapp.route.tls.termination` | Route TLS: `edge`, `passthrough`, `reencrypt`; vacío = sin TLS | `edge` |
| `webapp.route.tls.insecureEdgeTerminationPolicy` | Con edge: `Redirect` (HTTP→HTTPS), `Allow`, `None` | `Redirect` |
| `webapp.apiCustomersUrl` | Base URL for Customers API (browser, end with /api) | `""` |
| `webapp.apiBillsUrl` | Base URL for Bills API (browser, end with /api) | `""` |
| `webapp.apiRaidersUrl` | Base URL for Raiders API (optional) | `""` |
| `*.persistence.enabled` | Use PVC for SQLite data | `true` |
| `apiKeys.enabled` | Create Secret and inject API keys into APIs | `false` |
| `apiKeys.customers` / `bills` / `raiders` | API key per API (header `X-API-Key`) | `""` |
| `gateway.enabled` | Create Gateway API Gateway + HTTPRoutes | `false` |
| `gateway.className` | GatewayClass (e.g. `istio`, `openshift-gateway`) | `istio` |
| `gateway.existingGatewayName` | Use existing Gateway instead of creating one | `""` |
| `authorizationPolicy.enabled` | Istio AuthorizationPolicy: require `X-API-Key` header | `false` |

With default values, the webapp Apache proxies `/api-customers`, `/api-bills`, and `/api-raiders` to the internal API services, so only the webapp Route is needed and the "Unexpected token '<'" error is avoided.

### Securing with API keys

To secure endpoints with one API key per API:

1. Set `apiKeys.enabled: true` and provide keys (e.g. `--set apiKeys.customers=secret1 --set apiKeys.bills=secret2 --set apiKeys.raiders=secret3`). Keys are stored in Secret `nfl-wallet-api-keys` and injected as env `ApiKey` into each API.
2. Each .NET API validates the `X-API-Key` header when `ApiKey` is set. The frontend sends these keys by default (hardcoded or via `config.json` `apiKeys`).
3. In Swagger UI for each API, use **Authorize** and set the same key to test.
4. Optionally enable `authorizationPolicy.enabled` when using Istio so the mesh requires the header before traffic reaches the pod.
5. Optionally enable `gateway.enabled` to expose APIs via Gateway API (HTTPRoute) instead of only ClusterIP services.

### Levantar Helm con Connectivity Link (Gateway API)

Para exponer las APIs vía Gateway API (HTTPRoute / Connectivity Link) en lugar de solo Services:

1. **Cluster con Gateway API** (OpenShift tiene Gateway API; si usás Istio, asegurate de tener el GatewayClass instalado).

2. **Instalación con gateway habilitado**:

```bash
# Crear namespace
kubectl create namespace nfl-wallet

# Instalar con connectivity link (Gateway + HTTPRoutes para cada API)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet --set gateway.enabled=true

# En OpenShift suele usarse el GatewayClass de la plataforma (ej. openshift-gateway)
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.className=openshift-gateway

# Si ya tenés un Gateway creado en el namespace, reutilizarlo
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.existingGatewayName=mi-gateway
```

3. **Qué se crea**: un `Gateway` (si no usás `existingGatewayName`) y tres `HTTPRoute` que enlazan `/api-customers`, `/api-bills` y `/api-raiders` con los Services correspondientes. El frontend sigue expuesto por la Route de OpenShift (webapp) con edge + redirect.

## Uninstall

```bash
helm uninstall nfl-wallet -n nfl-wallet
```
