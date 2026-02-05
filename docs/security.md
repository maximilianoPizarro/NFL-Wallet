---
layout: default
title: Security (API Keys and Istio)
description: "API key validation and Istio AuthorizationPolicy for NFL Stadium Wallet (e.g. api-raiders-require-apikey)."
---

## API Key and Istio Security Scenario

The Helm chart supports securing the APIs with **API keys** and, when Istio is in use, with **Istio AuthorizationPolicy** so that only requests that send the `X-API-Key` header are allowed. This section describes that scenario and the resulting policy resource (e.g. for the Raiders wallet API).

### How It Works

1. **API keys** — You enable `apiKeys.enabled` and set one key per API (e.g. `apiKeys.raiders`). The chart creates a Secret `nfl-wallet-api-keys` and injects the key into each API deployment as the `ApiKey` environment variable.
2. **Application layer** — Each .NET API, when `ApiKey` is set, validates the `X-API-Key` request header and returns 401 if it is missing or wrong.
3. **Istio layer (optional)** — When `authorizationPolicy.enabled` is true, the chart creates **AuthorizationPolicy** resources (Istio `security.istio.io/v1`). You can enable the policy per API (e.g. only for the Raiders wallet) so that the mesh allows only requests that present the `X-API-Key` header before traffic reaches the pod.

### Enabling API Key and AuthorizationPolicy (e.g. Raiders only)

Example: enable API keys, require the header only for the **Raiders** API, and set the Raiders key:

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set apiKeys.enabled=true \
  --set apiKeys.raiders=nfl-wallet-raiders-key \
  --set authorizationPolicy.enabled=true \
  --set authorizationPolicy.requireForCustomers=false \
  --set authorizationPolicy.requireForBills=false \
  --set authorizationPolicy.requireForRaiders=true
```

The frontend is configured (e.g. in `client.js`) to send the same key in the `X-API-Key` header for Raiders; you can override keys via the webapp config.

### Istio AuthorizationPolicy: `api-raiders-require-apikey`

When the policy is enabled for the Raiders API, the chart creates an **AuthorizationPolicy** that restricts the **api-raiders** workload to requests that include the `x-api-key` header. The resource is in the same namespace as the release and is named **api-raiders-require-apikey**.

**Resource type and name (Istio):**

- **API version / kind:** `security.istio.io/v1`, `AuthorizationPolicy`
- **Name:** `api-raiders-require-apikey`

**Example YAML** (equivalent to what the chart renders):

```yaml
apiVersion: security.istio.io/v1
kind: AuthorizationPolicy
metadata:
  name: api-raiders-require-apikey
  labels:
    app: api-raiders
spec:
  selector:
    matchLabels:
      app: api-raiders
  action: ALLOW
  rules:
    - when:
        - key: request.headers[x-api-key]
          values: ["*"]
```

- **selector** — Applies to pods with label `app: api-raiders` (the Raiders wallet API).
- **action: ALLOW** — Only requests that match the rules are allowed; requests without the header are denied by the mesh.
- **rules** — Require that `request.headers[x-api-key]` is present (values `["*"]` mean “any non-empty value”). The actual key value is validated by the .NET API.

You can inspect the policy in the cluster with:

```bash
kubectl get authorizationpolicy api-raiders-require-apikey -n nfl-wallet -o yaml
```

Or in the OpenShift/Istio UI under **Istio Security** → **AuthorizationPolicies** (e.g. `security.istio.io/v1` / `AuthorizationPolicy` / `api-raiders-require-apikey`).

### Enabling for All Three APIs

To require the API key at the mesh level for Customers, Bills, and Raiders, set all three keys and leave the per-API flags at their default (true):

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set apiKeys.enabled=true \
  --set apiKeys.customers=your-customers-key \
  --set apiKeys.bills=your-bills-key \
  --set apiKeys.raiders=nfl-wallet-raiders-key \
  --set authorizationPolicy.enabled=true
```

The chart will then create three AuthorizationPolicies: `api-customers-require-apikey`, `api-bills-require-apikey`, and `api-raiders-require-apikey`, each requiring the `x-api-key` header for the corresponding workload.
