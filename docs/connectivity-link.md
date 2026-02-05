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

### Kuadrant RateLimitPolicy (opcional)

Si tienes el **operador Kuadrant** instalado en el clúster, puedes activar un límite de tasa para `/api-bills` (por defecto 100 peticiones por minuto):

```bash
helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
  --set gateway.enabled=true \
  --set gateway.rateLimitPolicy.enabled=true
```

Valores relacionados: `gateway.rateLimitPolicy.bills.limit` (default `100`), `gateway.rateLimitPolicy.bills.window` (default `1m`).

### Cómo probar el rate limit

1. **Desplegar con gateway y rate limit activados** (y Kuadrant instalado):

   ```bash
   helm upgrade nfl-wallet ./helm/nfl-wallet -n nfl-wallet \
     --set gateway.enabled=true \
     --set gateway.rateLimitPolicy.enabled=true
   ```

   Si usas API key en las APIs, incluye el header en las peticiones (ej. `-H "X-API-Key: tu-apikey"`).

2. **Obtener la URL del gateway** (OpenShift):

   ```bash
   oc get route -n nfl-wallet -l app.kubernetes.io/component=gateway -o jsonpath='{.items[0].spec.host}'
   ```

   O listar rutas y usar la que apunte al gateway: `oc get route -n nfl-wallet`.

3. **Enviar muchas peticiones a `/api-bills`** hasta superar el límite (por defecto 100 en 1 minuto). Ejemplo en PowerShell:

   ```powershell
   $host = "nfl-wallet-gateway-nfl-wallet.apps.tu-dominio.com"   # reemplaza por tu host
   $url = "https://$host/api-bills/"
   1..105 | ForEach-Object {
     $r = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -Headers @{ "X-API-Key" = "test" } -ErrorAction SilentlyContinue
     Write-Host "$_ : $($r.StatusCode)"
   }
   ```

   En Bash:

   ```bash
   HOST="nfl-wallet-gateway-nfl-wallet.apps.tu-dominio.com"
   for i in $(seq 1 105); do
     code=$(curl -s -o /dev/null -w "%{http_code}" -H "X-API-Key: test" "https://$HOST/api-bills/")
     echo "$i : $code"
   done
   ```

   Las primeras ~100 respuestas deberían ser **200**; a partir del límite verás **429** (Too Many Requests).

4. **Comprobar el estado del RateLimitPolicy**:

   ```bash
   kubectl get ratelimitpolicy -n nfl-wallet
   kubectl describe ratelimitpolicy nfl-wallet-api-bills-ratelimit -n nfl-wallet
   ```

   El recurso debe estar aplicado y, si Kuadrant está correctamente configurado, el límite se aplicará en el gateway.

### Connectivity Link overview

{% assign conn_img = '/connectivity link.png' | relative_url | replace: ' ', '%20' %}
[![Connectivity Link]({{ conn_img }})]({{ conn_img }}){: .doc-img-link}
*Gateway API and HTTPRoutes (Connectivity Link) exposing the webapp and APIs. Click to enlarge.*
