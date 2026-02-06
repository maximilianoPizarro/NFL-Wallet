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
| `webapp.mobileAppDownloadUrl` | URL for downloading the mobile app (APK) from the browser. Shown in the header and on the landing hero. Use a path like `/nfl-wallet.apk` to serve the APK from the same site (place the file in the frontend `public/` folder); or an external URL. Empty = link hidden. | `""` |
| `*.persistence.enabled` | Use PVC for SQLite data | `true` |
| `apiKeys.enabled` | Create Secret and inject API keys into APIs | `false` |
| `apiKeys.customers` / `bills` / `raiders` | API key per API (header `X-API-Key`) | `""` |
| `gateway.enabled` | Create Gateway API Gateway + HTTPRoutes | `false` |
| `gateway.className` | GatewayClass (e.g. `istio`, `openshift-gateway`) | `istio` |
| `gateway.existingGatewayName` | Use existing Gateway instead of creating one | `""` |
| `gateway.route.enabled` | OpenShift Route for the Gateway (Edge + Redirect) | `true` when gateway.enabled |
| `gateway.route.host` | Gateway Route host | `""` |
| `gateway.route.targetPort` | Gateway Service port (name or number) | `http` |
| `gateway.rateLimitBills.enabled` | Istio EnvoyFilter rate limit for api-bills workload | `false` |
| `gateway.rateLimitBills.permitLimit`, `fillIntervalSeconds` | Token bucket (Istio) | `100`, `60` |
| `gateway.rateLimitPolicy.enabled` | Kuadrant RateLimitPolicy for HTTPRoute nfl-wallet-api-bills | `false` |
| `gateway.rateLimitPolicy.bills.limit`, `window` | Kuadrant: limit and window (e.g. `100`, `1m`) | `100`, `1m` |
| `gateway.rateLimitPolicy.bills.conditions`, `counterExpressions` | Kuadrant: conditions and counters (optional) | path `/api-bills`, `[]` |
| `authorizationPolicy.enabled` | Istio AuthorizationPolicy: require `X-API-Key` header | `false` |
| `authorizationPolicy.requireForCustomers` / `requireForBills` / `requireForRaiders` | Apply policy only to these APIs (when enabled) | `true` each |
| `topology.applicationName` | Application name for OpenShift Topology grouping (`app.kubernetes.io/part-of`) | `nfl-wallet` |
| `observability.rhobs.enabled` | Deploy RHOBS resources (ThanosQuerier, PodMonitor/ServiceMonitor gateway, UIPlugin) | `false` |
| `observability.rhobs.namespace` | Namespace for RHOBS resources (e.g. `openshift-cluster-observability-operator`) | `openshift-cluster-observability-operator` |
| `observability.rhobs.thanosQuerier.enabled` | Create ThanosQuerier for the stack | `false` |
| `observability.rhobs.podMonitorGateway.enabled` | Create PodMonitor (RHOBS) for gateway metrics (port 15020) | `false` |
| `observability.rhobs.serviceMonitorGateway.enabled` | Create ServiceMonitor (RHOBS) for gateway metrics (port 15090) | `false` |
| `observability.rhobs.uiPlugin.enabled` | Create UIPlugin to enable Monitoring UI in OpenShift console | `false` |

### Observability (RHOBS)

When using **Red Hat OpenShift Observability** (Cluster Observability Operator), you can render ThanosQuerier, PodMonitor/ServiceMonitor for the gateway, and the Monitoring UIPlugin from the chart instead of applying separate YAMLs:

```bash
helm install nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set observability.rhobs.enabled=true \
  --set observability.rhobs.thanosQuerier.enabled=true \
  --set observability.rhobs.podMonitorGateway.enabled=true \
  --set observability.rhobs.uiPlugin.enabled=true
```

Resources are created in `observability.rhobs.namespace` (default `openshift-cluster-observability-operator`). Ensure that namespace exists and the release has permission to create resources there. The gateway monitors require `gateway.enabled=true`.

### Package chart and publish to docs (GitHub Pages Helm repo)

To generate the chart package (`.tgz`) and `index.yaml` inside the `docs/` folder so they can be served as a Helm repo via GitHub Pages:

1. **From the repo root**, run:

   **Linux/macOS:**
   ```bash
   chmod +x scripts/helm-package-docs.sh
   ./scripts/helm-package-docs.sh
   ```

   **Windows (PowerShell):**
   ```powershell
   .\scripts\helm-package-docs.ps1
   ```

2. This will:
   - Run `helm package helm/nfl-wallet --destination docs/` → creates `docs/nfl-wallet-<version>.tgz`
   - Run `helm repo index docs --url https://maximilianopizarro.github.io/NFL-Wallet [--merge docs/index.yaml]` → creates or updates `docs/index.yaml`

3. **Commit and push** `docs/nfl-wallet-*.tgz` and `docs/index.yaml`.

4. Users can add the repo and install:
   ```bash
   helm repo add nfl-wallet https://maximilianopizarro.github.io/NFL-Wallet
   helm repo update
   helm install nfl-wallet nfl-wallet/nfl-wallet -n nfl-wallet
   ```

To use a different repo URL, set `HELM_REPO_URL` (e.g. `export HELM_REPO_URL=https://youruser.github.io/NFL-Wallet`) before running the script.

### Publish to Artifact Hub

To list the chart on [Artifact Hub](https://artifacthub.io):

1. **Package and publish the repo** (steps above) so the chart is available at a public URL (e.g. GitHub Pages).
2. In **Artifact Hub**, sign in → **Control panel** → **Repositories** → **Add** → choose **Helm**.
3. Enter the **repository URL** (e.g. `https://maximilianopizarro.github.io/NFL-Wallet`).
4. Save. Artifact Hub indexes the chart and shows it in search; the chart’s `Chart.yaml` already includes Artifact Hub annotations (category, license, links, changes).

See the docs site: [Publish to Artifact Hub](https://maximilianopizarro.github.io/NFL-Wallet/artifact-hub.html) for the full steps (including optional Verified Publisher).

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

3. **Rate limit at connectivity link (no app code change):**
   - **Kuadrant:** Set `gateway.rateLimitPolicy.enabled: true` (and `gateway.enabled: true`). The chart creates a `RateLimitPolicy` (kuadrant.io/v1) that targets the HTTPRoute `nfl-wallet-api-bills`, with configurable limit/window and optional conditions and counters (e.g. per-user with `auth.identity.username`). Requires [Kuadrant](https://docs.kuadrant.io/) operator installed.
   - **Istio only:** Set `gateway.rateLimitBills.enabled: true` for an EnvoyFilter-based local rate limit on the api-bills workload.

4. **What gets created**: a `Gateway` (if not using `existingGatewayName`), four `HTTPRoute`s—one for the **webapp** (path `/`) and three for the APIs (`/api-customers`, `/api-bills`, `/api-raiders`)—and an **OpenShift Route** `nfl-wallet-gateway` with TLS Edge + Redirect to expose the Gateway. Each API HTTPRoute uses a **URLRewrite** filter so that `/api-customers`, `/api-bills`, `/api-raiders` are rewritten to `/api` before reaching the backends (fixes 404 when calling e.g. `https://gateway-host/api-customers/Customers`). The frontend remains exposed via the webapp Route. You can disable the gateway Route with `--set gateway.route.enabled=false` if traffic is already entering through another path.

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

### Testing the API (gateway + X-API-Key)

When the gateway is enabled and api-bills is protected by Istio AuthorizationPolicy or Kuadrant AuthPolicy (X-API-Key required), you can verify behaviour with `curl`. Replace `GATEWAY_HOST` with your gateway host (e.g. from `kubectl get route -n nfl-wallet`).

**Without API key** — expect **403** (Forbidden):

```bash
curl -s -o /dev/null -w "%{http_code}" "https://GATEWAY_HOST/api-bills/Wallet/balance/1"
echo
# Example output: 403
```

**With API key** — expect **200** (and JSON body if you omit `-o /dev/null`):

```bash
curl -s -o /dev/null -w "%{http_code}" -H "X-API-Key: test" "https://GATEWAY_HOST/api-bills/Wallet/balance/1"
echo
# Example output: 200
```

**Full response (with header):**

```bash
curl -s -H "X-API-Key: test" "https://GATEWAY_HOST/api-bills/Wallet/balance/1"
# Example: {"id":1,"customerId":1,"currency":"USD","availableBalance":2847.25,...}
```

## Uninstall

```bash
helm uninstall nfl-wallet -n nfl-wallet
```
