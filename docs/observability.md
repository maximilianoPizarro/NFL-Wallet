---
layout: default
title: Observability
description: "Configure observability for NFL Stadium Wallet with the Cluster Observability Operator: gateway metrics, ThanosQuerier, PodMonitor, ServiceMonitor, and UIPlugin."
---

## Observability with Cluster Observability Operator

This section describes how to configure **observability** for the NFL-Wallet gateway so you can view **Total Requests**, **Successful Requests**, and **Error Rate** (and other Istio/Envoy metrics) in the OpenShift observability UI. The configuration uses the **Cluster Observability Operator** (RHOBS) and resources from the repository’s `config/observability` folder.

---

## What You Need

### Cluster Observability Operator

- The **Cluster Observability Operator** must be installed on the cluster (Red Hat OpenShift Cluster Observability Operator / RHOBS).
- The operator provides:
  - **MonitoringStack** and related CRDs (`monitoring.rhobs`).
  - **ThanosQuerier** for querying metrics.
  - **ServiceMonitor** and **PodMonitor** (API `monitoring.rhobs/v1`) so Prometheus scrapes targets.
  - **UIPlugin** (`observability.openshift.io/v1alpha1`) to enable the Monitoring UI in the OpenShift console.

### Namespace and gateway

- The namespace **`openshift-cluster-observability-operator`** must exist (it is typically created when the operator is installed).
- The **Gateway** for NFL-Wallet must be deployed (e.g. via the Helm chart with `gateway.enabled=true`) so gateway proxy pods exist in `nfl-wallet` and expose metrics on port 15020 (or 15090 on the Service if using ServiceMonitor).

---

## How to Configure Observability

All manifests are in the **`config/observability`** folder in the repository. Apply them with `kubectl apply -f config/observability/` (or apply each file as needed).

### 1. ThanosQuerier

The ThanosQuerier is the query component used by the monitoring stack. The sample manifest uses a selector that matches a MonitoringStack labeled with `app.kubernetes.io/part-of: nfl-wallet`. Ensure your MonitoringStack has that label, or adjust `spec.selector.matchLabels` in the file to match your stack.

```bash
kubectl apply -f config/observability/thanos-querier-rhobs.yaml
```

### 2. Gateway monitors (nfl-wallet traffic)

These monitors tell the operator’s Prometheus to scrape the **gateway proxy** (Istio/Envoy) in `nfl-wallet`, so metrics such as request count and response codes are collected.

- **PodMonitor** (recommended): scrapes gateway **pods** on port 15020.

  ```bash
  kubectl apply -f config/observability/pod-monitor-gateway-rhobs.yaml
  ```

- **ServiceMonitor**: use if the gateway **Service** exposes the metrics port (e.g. 15090).

  ```bash
  kubectl apply -f config/observability/service-monitor-gateway-rhobs.yaml
  ```

After application, the operator’s Prometheus will scrape the gateway and you can query metrics (e.g. `istio_requests_total`) and view Total Requests, Successful Requests, and Error Rate in the observability UI.

### 3. UIPlugin (enable the Monitoring UI)

Enable the **Monitoring** UI plugin in the OpenShift console so you can use the observability UI (and optionally configure the monitoring datasource if required by your operator version).

```bash
kubectl apply -f config/observability/ui-plugin.yaml
```

The UIPlugin resource must have `metadata.name: monitoring` and `spec.type: Monitoring` (as in the provided `ui-plugin.yaml`). If the operator reports that “monitoring configuration can not be empty”, refer to the Cluster Observability Operator documentation for any required `spec` fields (e.g. reference to a MonitoringStack or ThanosQuerier).

---

## Applying Everything from `config/observability`

From the **repository root** you can apply all observability resources in one go:

```bash
kubectl apply -f config/observability/
```

If the UIPlugin fails validation (e.g. empty name or missing spec), fix the indicated field and re-apply only that file:

```bash
kubectl apply -f config/observability/ui-plugin.yaml
```

---

## Files in `config/observability`

| File | Description |
|------|-------------|
| `thanos-querier-rhobs.yaml` | ThanosQuerier (monitoring.rhobs/v1alpha1). Selector should match your MonitoringStack labels. |
| `pod-monitor-gateway-rhobs.yaml` | PodMonitor (monitoring.rhobs/v1) for the gateway in `nfl-wallet` (port 15020). |
| `service-monitor-gateway-rhobs.yaml` | ServiceMonitor (monitoring.rhobs/v1) for the gateway in `nfl-wallet` (port 15090). |
| `ui-plugin.yaml` | UIPlugin (observability.openshift.io/v1alpha1) for the Monitoring UI. |
| `README.md` | Short reference for the same steps (in the repo). |

---

## Observability and traffic analysis in the console

The following screenshots show the observability UI and traffic analysis once the Cluster Observability Operator, gateway monitors, and UIPlugin are configured.

### Observability dashboard

[![Observability overview]({{ '/obvservability.png' | relative_url }})]({{ '/obvservability.png' | relative_url }}){: .doc-img-link}
*Observability overview in the OpenShift console: monitoring stack and metrics from the NFL-Wallet gateway. Click to enlarge.*

[![Observability metrics]({{ '/obvservability2.png' | relative_url }})]({{ '/obvservability2.png' | relative_url }}){: .doc-img-link}
*Gateway metrics (e.g. request rate, success and error rates) available in the observability UI after PodMonitor or ServiceMonitor is applied. Click to enlarge.*

[![Observability details]({{ '/obvservability3.png' | relative_url }})]({{ '/obvservability3.png' | relative_url }}){: .doc-img-link}
*Detailed observability view with Istio/Envoy metrics for the NFL-Wallet gateway. Click to enlarge.*

### Traffic analysis

{% assign traffic_img = '/traffic analysis.png' | relative_url | replace: ' ', '%20' %}
[![Traffic analysis]({{ traffic_img }})]({{ traffic_img }}){: .doc-img-link}
*Traffic analysis view showing request flow, latency, and response codes for the gateway and backend services. Use this to inspect Total Requests, Successful Requests, and Error Rate. Click to enlarge.*

---

## Prometheus queries (reference)

Once the gateway is being scraped, you can use queries such as:

| Metric | Example query |
|--------|----------------|
| **Total Requests** (rate) | `sum(rate(istio_requests_total[5m]))` (optionally filter by namespace or labels) |
| **Successful Requests** (2xx) | `sum(rate(istio_requests_total{response_code=~"2.."}[5m]))` |
| **Error Rate** | `sum(rate(istio_requests_total{response_code=~"5.."}[5m])) / sum(rate(istio_requests_total[5m]))` |

Adjust labels (e.g. `destination_workload_namespace`, `destination_workload`) to match your mesh. The observability UI (after UIPlugin is enabled and configured) can use the same metric names.
