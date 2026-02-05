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
| `gateway.hostnames` | Hostnames for all HTTPRoutes (needed for Kuadrant; ensures `/` and APIs match the gateway host) | `[]` |
| `gateway.route.enabled` | Create OpenShift Route for the Gateway (edge + redirect) | `true` when gateway enabled |
| `gateway.route.name` | Name of the Route resource | `nfl-wallet-gateway` |
| `gateway.route.targetPort` | Target port of the Gateway Service | `http` |

**Two gateway routes?** If you see both **nfl-gateway** (→ nfl-wallet-gateway-istio) and **nfl-wallet-gateway** (→ nfl-wallet-gateway), the first is usually created by the platform (Istio/OpenShift) and the second by this chart. To keep only one gateway URL, set `gateway.route.enabled: false` so the chart does not create a Route; use the platform route (e.g. nfl-gateway) and set `gateway.hostnames` to that host when using Kuadrant policies.

### What gets created

- **Gateway** (if `gateway.existingGatewayName` is not set): one Gateway resource.
- **HTTPRoutes**: `nfl-wallet-webapp` (path `/`), `nfl-wallet-api-customers`, `nfl-wallet-api-bills`, `nfl-wallet-api-raiders` with path prefix and URL rewrite to `/api`. When `gateway.hostnames` is set, all four HTTPRoutes get that hostname so the gateway matches the public host (e.g. for a single platform route).
- **OpenShift Route** `nfl-wallet-gateway`: exposes the Gateway with TLS edge and HTTP→HTTPS redirect (when `gateway.route.enabled` is true).

The frontend remains available via the webapp Route as before; the gateway Route provides an alternate entry point for the same backends.

### Install AuthPolicy and RateLimitPolicy together (Kuadrant)

To install **both** Kuadrant AuthPolicy (X-API-Key for `/api-bills`) and RateLimitPolicy (e.g. 100/min) you must set all of these to `true`:

| Value | Required for |
|-------|----------------|
| `gateway.enabled=true` | Gateway, HTTPRoutes, both policies |
| `gateway.rateLimitPolicy.enabled=true` | RateLimitPolicy |
| `gateway.authPolicy.enabled=true` | AuthPolicy |
| `gateway.authPolicy.bills.enabled=true` | AuthPolicy for api-bills |

**One-liner (replace the host or use the next block):**

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.rateLimitPolicy.enabled=true \
  --set gateway.authPolicy.enabled=true \
  --set gateway.authPolicy.bills.enabled=true \
  --set gateway.hostnames[0]=nfl-gateway-nfl-wallet.apps.cluster-XXX.sandbox.opentlc.com
```

**With host from the Route (Bash):**

```bash
GWHOST=$(kubectl get route -n nfl-wallet -l app.kubernetes.io/component=gateway -o jsonpath='{.items[0].spec.host}')
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.rateLimitPolicy.enabled=true \
  --set gateway.authPolicy.enabled=true \
  --set gateway.authPolicy.bills.enabled=true \
  --set gateway.hostnames[0]="$GWHOST"
```

Then verify:

```bash
kubectl get authpolicy,ratelimitpolicy -n nfl-wallet
```

### Kuadrant RateLimitPolicy (optional)

If the **Kuadrant operator** is installed in the cluster, you can enable a rate limit for `/api-bills` (default 100 requests per minute):

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.rateLimitPolicy.enabled=true
```

Related values: `gateway.rateLimitPolicy.bills.limit` (default `100`), `gateway.rateLimitPolicy.bills.window` (default `1m`).

### How to test the rate limit

1. **Deploy with gateway and rate limit enabled** (and Kuadrant installed):

   ```bash
   helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
     --set gateway.enabled=true \
     --set gateway.rateLimitPolicy.enabled=true
   ```

   If you use API keys for the APIs, include the header in requests (e.g. `-H "X-API-Key: your-apikey"`).

2. **Get the gateway URL** (OpenShift):

   ```bash
   oc get route -n nfl-wallet -l app.kubernetes.io/component=gateway -o jsonpath='{.items[0].spec.host}'
   ```

   Or list routes and use the one that targets the gateway: `oc get route -n nfl-wallet`.

3. **Send requests to `/api-bills`** to verify behavior. Example with 20 requests (PowerShell):

   ```powershell
   $host = "nfl-wallet-gateway-nfl-wallet.apps.your-domain.com"   # replace with your host
   $url = "https://$host/api-bills/"
   1..20 | ForEach-Object {
     $r = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -Headers @{ "X-API-Key" = "test" } -ErrorAction SilentlyContinue
     Write-Host "$_ : $($r.StatusCode)"
   }
   ```

   Bash:

   ```bash
   HOST="nfl-wallet-gateway-nfl-wallet.apps.your-domain.com"
   for i in $(seq 1 20); do
     code=$(curl -s -o /dev/null -w "%{http_code}" -H "X-API-Key: test" "https://$HOST/api-bills/")
     echo "$i : $code"
   done
   ```

   With the default limit (100 per minute), the first 100 responses in a minute are **200**; after that you get **429** (Too Many Requests). To trigger 429, run more than 100 requests in one minute (e.g. `seq 1 105`).

4. **Check the RateLimitPolicy status**:

   ```bash
   kubectl get ratelimitpolicy -n nfl-wallet
   kubectl describe ratelimitpolicy nfl-wallet-api-bills-ratelimit -n nfl-wallet
   ```

   The resource should be applied; if Kuadrant is configured correctly, the limit is enforced at the gateway.

   **Note:** The rate limiting component may run in the Kuadrant operator namespace (e.g. `kuadrant-system`) or inside the gateway data plane, so you may not see a dedicated "limitator" pod in the `nfl-wallet` namespace topology.

### Kuadrant AuthPolicy for /api-bills (optional)

If the **Kuadrant operator** is installed, you can secure the `/api-bills` route with an **AuthPolicy** that requires the **X-API-Key** header (any non-empty value), with a custom JSON 403 body when the header is missing. This uses Kuadrant/Authorino at the gateway instead of Istio AuthorizationPolicy on the workload.

Enable it with:

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.authPolicy.enabled=true \
  --set gateway.authPolicy.bills.enabled=true
```

When this is enabled, the chart does **not** create the Istio `AuthorizationPolicy` for api-bills (Kuadrant enforces the same rule at the gateway). The policy allows requests that include any non-empty `X-API-Key` header and returns 403 with a configurable JSON body otherwise.

**Important:** If the AuthConfig in `istio-system` shows **"No hosts linked to the resource"** (AuthPolicy stays `Enforced: False`), set **`gateway.hostnames`** to the gateway's public hostname so Kuadrant can link the AuthConfig to your host. Get the host from the Route, then upgrade with it:

```bash
GWHOST=$(kubectl get route -n nfl-wallet -l app.kubernetes.io/component=gateway -o jsonpath='{.items[0].spec.host}')
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.authPolicy.enabled=true \
  --set gateway.authPolicy.bills.enabled=true \
  --set gateway.hostnames[0]="$GWHOST"
```

After a short while, `kubectl get authconfig -n istio-system` should show the AuthConfig as READY (1/1) and the AuthPolicy as Enforced.

| Value | Description | Default |
|-------|-------------|---------|
| `gateway.hostnames` | List of hostnames for the gateway (required for Kuadrant AuthConfig to link; use the Route host) | `[]` |
| `gateway.authPolicy.enabled` | Enable Kuadrant AuthPolicy for the gateway routes | `false` |
| `gateway.authPolicy.bills.enabled` | Apply AuthPolicy to the api-bills HTTPRoute | `false` |
| `gateway.authPolicy.bills.unauthorizedBody` | JSON string for 403 response body | `{"error":"Forbidden","message":"Missing or invalid X-API-Key header."}` |

**If you get 500 on `/api-bills/...` (with or without X-API-Key)** and the AuthConfig in `istio-system` has `spec.hosts` set to a hash instead of your gateway host, patch it so Authorino can match the request:

```bash
# Find the AuthConfig that backs the bills AuthPolicy (name is a hash)
kubectl get authconfig -n istio-system -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.spec.hosts[0]}{"\n"}{end}' | head -20

# Patch the one that has the hash as host (replace HASH with the name, e.g. d17c85a098a189a7f6e791f60c3fdeec93daaf45a9e4986b67391d9d24350ece)
kubectl patch authconfig HASH -n istio-system --type=json -p='[{"op":"replace","path":"/spec/hosts","value":["nfl-gateway-nfl-wallet.apps.cluster-zrg7z.zrg7z.sandbox489.opentlc.com"]}]'
```

Use your actual gateway host in `value`. After the patch, requests without X-API-Key should return **403** and with the header **200** (or whatever the API returns).

**If you get 500 and the AuthPolicy status shows `Enforced: False`** ("waiting for AuthConfig to sync"):

1. The gateway may be calling Authorino before the AuthConfig is ready, which can result in 500. Wait a minute and check again: `kubectl get authpolicy nfl-wallet-api-bills-auth -n nfl-wallet -o jsonpath='{.status.conditions}'`.
2. When `Enforced` is `True`, requests without `X-API-Key` should get **403** with your JSON body; with a valid header they should reach the backend (200 or 404/5 from the API).
3. To see why the policy is not enforced, list and describe the AuthConfig that Kuadrant creates: `kubectl get authconfig -n nfl-wallet` and `kubectl describe authconfig <name> -n nfl-wallet`. Check the AuthConfig status and any Authorino/Kuadrant operator logs.

### Connectivity Link overview

{% assign conn_img = '/connectivity link.png' | relative_url | replace: ' ', '%20' %}
[![Connectivity Link]({{ conn_img }})]({{ conn_img }}){: .doc-img-link}
*Gateway API and HTTPRoutes (Connectivity Link) exposing the webapp and APIs. Click to enlarge.*

{% assign conn_auth_img = '/connectivity link with auth and limit.png' | relative_url | replace: ' ', '%20' %}
[![Connectivity Link with Auth and Rate Limit]({{ conn_auth_img }})]({{ conn_auth_img }}){: .doc-img-link}
*Connectivity Link with Kuadrant AuthPolicy (X-API-Key) and RateLimitPolicy on `/api-bills`. Click to enlarge.*
