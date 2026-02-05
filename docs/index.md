---
layout: default
title: Home
description: "NFL Stadium Wallet — Centralized digital wallet for use at NFL stadiums. Pay, load balance, and manage your money at Buffalo Bills and Las Vegas Raiders venues."
---

<div class="hero">
  <img src="{{ '/National_Football_League_logo.png' | relative_url }}" alt="NFL" class="logo-hero">
  <h1>NFL Stadium Wallet</h1>
  <p>{{ site.description }}</p>
</div>

<p align="left">
  <img src="https://img.shields.io/badge/redhat-CC0000?style=for-the-badge&logo=redhat&logoColor=white" alt="Red Hat">
  <img src="https://img.shields.io/badge/kubernetes-%23326ce5.svg?style=for-the-badge&logo=kubernetes&logoColor=white" alt="Kubernetes">
  <img src="https://img.shields.io/badge/helm-0db7ed?style=for-the-badge&logo=helm&logoColor=white" alt="Helm">
  <img src="https://img.shields.io/badge/shell_script-%23121011.svg?style=for-the-badge&logo=gnu-bash&logoColor=white" alt="Shell">
  <a href="{{ '/architecture.html#running-with-podman-compose' | relative_url }}"><img src="https://img.shields.io/badge/podman-892CA0?style=for-the-badge&logo=podman&logoColor=white" alt="Podman"></a>
  <a href="{{ site.github_repo }}" target="_blank" rel="noopener"><img src="https://img.shields.io/badge/Open%20in%20Dev%20Spaces-CC0000?style=for-the-badge&logo=redhatopenshift&logoColor=white" alt="Open in Dev Spaces"></a>
  <a href="https://www.linkedin.com/in/maximiliano-gregorio-pizarro-consultor-it"><img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn" /></a>
  <a href="https://artifacthub.io/packages/search?repo=nfl-wallet"><img src="https://artifacthub.io/badge/repository/nfl-wallet" alt="Artifact Hub" /></a>
</p>

## Purpose

**NFL Stadium Wallet** is a centralized digital wallet solution designed for use at NFL stadiums. It allows fans to:

- **View customer data** and link it to one or more team-specific wallets (e.g., Buffalo Bills, Las Vegas Raiders).
- **Check balances and transactions** per wallet.
- **Load balance** and **pay** from a wallet at the venue.

A single front-end application talks to a **Customers API** (for identity and customer list) and to **per-team Wallet APIs** (Buffalo Bills and Las Vegas Raiders). This way, one wallet experience can be reused across stadiums that adopt the same APIs.

### Wallet app (landing and flows)

<p class="doc-img-expand-hint">Click any image to open full size.</p>

[![Wallet landing]({{ '/walletlanding.png' | relative_url }})]({{ '/walletlanding.png' | relative_url }}){: .doc-img-link}
*Landing page.*

[![Wallet customer list]({{ '/wallet.png' | relative_url }})]({{ '/wallet.png' | relative_url }}){: .doc-img-link}
*Customer list.*

[![Wallet balances]({{ '/wallet2.png' | relative_url }})]({{ '/wallet2.png' | relative_url }}){: .doc-img-link}
*Customer wallets — Buffalo Bills and Las Vegas Raiders.*

[![Wallet pay flow]({{ '/wallet3.png' | relative_url }})]({{ '/wallet3.png' | relative_url }}){: .doc-img-link}
*Pay from QR flow.*

[![Wallet load]({{ '/wallet4.png' | relative_url }})]({{ '/wallet4.png' | relative_url }}){: .doc-img-link}
*Load balance.*

## What You Can Do With This Solution

| Option | Description |
|--------|-------------|
| **Run locally** | Use **Podman Compose** to run the full stack (webapp + three APIs) on your machine. |
| **Develop in the cloud** | Use **Red Hat OpenShift Dev Spaces** (or CodeReady Workspaces) to build and run the app from a browser with a single workspace. |
| **Deploy to Kubernetes** | Use the provided **Helm chart** to deploy the webapp and APIs to any Kubernetes or OpenShift cluster. |
| **Expose via Gateway** | Enable the **Connectivity Link** (Gateway API + HTTPRoutes) and an OpenShift Route for the gateway with TLS edge and redirect. |
| **Secure with API keys** | Optionally enable **API key** validation in the apps and **Istio AuthorizationPolicy** (e.g. require `X-API-Key` for the Raiders wallet API). |

## Quick Links

- [Architecture]({{ '/architecture.html' | relative_url }}) — Components, Podman Compose, and Dev Spaces
- [Deployment]({{ '/deployment.html' | relative_url }}) — Helm chart install and values
- [Connectivity Link]({{ '/connectivity-link.html' | relative_url }}) — Gateway, HTTPRoutes, Kuadrant AuthPolicy and RateLimitPolicy
- [Security]({{ '/security.html' | relative_url }}) — API keys and Istio AuthorizationPolicy (e.g. `api-raiders-require-apikey`)
- [Publish to Artifact Hub]({{ '/artifact-hub.html' | relative_url }}) — Package the chart and list it on Artifact Hub
- [**GitHub repository**]({{ site.github_repo }}) — Source code

## Technology Stack

- **Frontend:** Vue 3, Vite, vue-router; served by Apache (UBI8).
- **Backend:** Three .NET 8.0 ASP.NET Core APIs (Customers, Buffalo Bills wallet, Las Vegas Raiders wallet).
- **Data:** SQLite (one database per API).
- **Containers:** Podman / OpenShift; images can be built and pushed to Quay.
