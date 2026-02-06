---
layout: default
title: Security (API Keys and Policies)
description: "API key validation and two policy types: Istio AuthorizationPolicy and Kuadrant AuthPolicy with Authorino (Connectivity Link)."
---

This page describes two ways to enforce **API key** (X-API-Key) at the infrastructure layer, in addition to optional application-level validation:

1. **Istio AuthorizationPolicy** — Policy applied at the service mesh (workload). The chart creates `AuthorizationPolicy` resources (e.g. `api-raiders-require-apikey`) when the gateway is not using Kuadrant AuthPolicy for that route.
2. **AuthPolicy with Authorino (Connectivity Link)** — Policy applied at the **gateway** via Kuadrant/Authorino. The chart can create an **AuthPolicy** that requires the X-API-Key header for a given HTTPRoute (e.g. `/api-bills`), with a custom 403 body. This demonstrates gateway-level auth with Red Hat Connectivity Link.

Both approaches allow only requests that present the `X-API-Key` header; the difference is where the check runs (mesh vs. gateway) and which CRDs are used.

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

---

## AuthPolicy with API Key (Authorino / Connectivity Link)

When the **Kuadrant operator** (Connectivity Link) is installed and the gateway is enabled, you can secure a route with an **AuthPolicy** that requires the **X-API-Key** header. Authorization is enforced at the **gateway** by **Authorino**, not at the workload by Istio. This is the second policy type: gateway-level auth with a custom 403 body when the header is missing.

### How It Works

- **AuthPolicy** (Kuadrant `kuadrant.io` API) targets an HTTPRoute (e.g. `nfl-wallet-api-bills`). Kuadrant creates an **AuthConfig** that Authorino uses to check the request.
- Requests without a valid X-API-Key header receive **403 Forbidden** with a configurable JSON body (e.g. `{"error":"Forbidden","message":"Missing or invalid X-API-Key header."}`).
- When AuthPolicy is enabled for a route (e.g. api-bills), the chart does **not** create the Istio `AuthorizationPolicy` for that API (Kuadrant enforces at the gateway instead).

### Enabling AuthPolicy for /api-bills

Prerequisites: gateway enabled, Kuadrant operator installed, and `gateway.hostnames` set to the gateway’s public host (so the AuthConfig links correctly). See [Connectivity Link]({{ '/connectivity-link.html' | relative_url }}#kuadrant-authpolicy-for-apibills-optional) for hostname setup and troubleshooting.

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.authPolicy.enabled=true \
  --set gateway.authPolicy.bills.enabled=true
```

Set `gateway.hostnames[0]` to your gateway Route host if the AuthPolicy stays **Enforced: False** (e.g. get host with `kubectl get route -n nfl-wallet -l app.kubernetes.io/component=gateway -o jsonpath='{.items[0].spec.host}'`).

### AuthPolicy resource: YAML created by the chart

The chart creates an **AuthPolicy** (Kuadrant `kuadrant.io/v1`) in the release namespace named **`nfl-wallet-api-bills-auth`**. It targets the HTTPRoute `nfl-wallet-api-bills` and configures Authorino to require the X-API-Key header (OPA Rego) and return a custom 403 JSON body when the header is missing or empty.

**Example YAML** (equivalent to what the chart renders when `gateway.authPolicy.bills.enabled=true`):

```yaml
apiVersion: kuadrant.io/v1
kind: AuthPolicy
metadata:
  name: nfl-wallet-api-bills-auth
  labels:
    app: api-bills
    app.kubernetes.io/component: authpolicy
spec:
  targetRef:
    group: gateway.networking.k8s.io
    kind: HTTPRoute
    name: nfl-wallet-api-bills
  rules:
    authorization:
      require-apikey:
        opa:
          rego: |
            allow = true {
              input.context.request.http.headers["x-api-key"] != ""
            }
            allow = true {
              input.context.request.http.headers["X-Api-Key"] != ""
            }
    response:
      unauthorized:
        body:
          value: '{"error":"Forbidden","message":"Missing or invalid X-API-Key header."}'
        headers:
          content-type:
            value: application/json
```

- **targetRef** — Binds this policy to the HTTPRoute `nfl-wallet-api-bills`, so gateway traffic to `/api-bills` is subject to auth.
- **authorization (OPA)** — Allows the request only if the `x-api-key` or `X-Api-Key` header is non-empty; otherwise Authorino denies it.
- **response.unauthorized** — 403 response with the JSON body and `Content-Type: application/json` when the header is missing or invalid.

You can inspect the live resource with:

```bash
kubectl get authpolicy -n nfl-wallet
kubectl get authpolicy nfl-wallet-api-bills-auth -n nfl-wallet -o yaml
```

When **Enforced** is True, requests to the gateway’s `/api-bills` path without the header get 403; with the header they reach the backend (and the .NET API can still validate the key value if `apiKeys.bills` is set).
