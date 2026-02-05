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

## Purpose

**NFL Stadium Wallet** is a centralized digital wallet solution designed for use at NFL stadiums. It allows fans to:

- **View customer data** and link it to one or more team-specific wallets (e.g., Buffalo Bills, Las Vegas Raiders).
- **Check balances and transactions** per wallet.
- **Load balance** and **pay** from a wallet at the venue.

A single front-end application talks to a **Customers API** (for identity and customer list) and to **per-team Wallet APIs** (Buffalo Bills and Las Vegas Raiders). This way, one wallet experience can be reused across stadiums that adopt the same APIs.

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
- [Connectivity Link]({{ '/connectivity-link.html' | relative_url }}) — Gateway and HTTPRoute commands
- [Security]({{ '/security.html' | relative_url }}) — API keys and Istio AuthorizationPolicy (e.g. `api-raiders-require-apikey`)

## Technology Stack

- **Frontend:** Vue 3, Vite, vue-router; served by Apache (UBI8).
- **Backend:** Three .NET 8.0 ASP.NET Core APIs (Customers, Buffalo Bills wallet, Las Vegas Raiders wallet).
- **Data:** SQLite (one database per API).
- **Containers:** Podman / OpenShift; images can be built and pushed to Quay.
