---
layout: default
title: Architecture
description: "Architecture of NFL Stadium Wallet: Vue frontend, .NET APIs, Podman Compose, and Red Hat OpenShift Dev Spaces."
---

## Architecture Overview

The solution is built in three layers:

| Layer | Technology | Description |
|-------|------------|-------------|
| **Frontend** | Vue 3, Vite, vue-router | SPA served by Apache (UBI8 httpd-24). |
| **APIs** | .NET 8.0, ASP.NET Core | Three services: **ApiCustomers**, **ApiWalletBuffaloBills**, **ApiWalletLasVegasRaiders**. Path base `/api`, Swagger at `/api/swagger`. |
| **Data** | SQLite | One database per API: `customers.db`, `buffalobills.db`, `lasvegasraiders.db`. |

The frontend calls the Customers API for the customer list and details, and the two Wallet APIs for balance and transactions per team. All APIs support CORS for the frontend origin.

---

## Running With Podman Compose

You can run the full stack locally with **Podman Compose**. The `podman-compose.yml` at the repo root defines four services:

- **api-customers** — Customers API (port 5001)
- **api-buffalo-bills** — Buffalo Bills wallet API (port 5002)
- **api-las-vegas-raiders** — Las Vegas Raiders wallet API (port 5003)
- **webapp** — Vue frontend (port 5160)

**Commands:**

```bash
# From the repo root
podman-compose up -d --build

# Open the app
# http://localhost:5160
```

The webapp is built with build args so it points to `http://localhost:5001/api`, `http://localhost:5002/api`, and `http://localhost:5003/api` for the three APIs. Data is persisted in named volumes.

---

## Running With Red Hat OpenShift Dev Spaces

The repo includes a **devfile.yaml** for **Red Hat OpenShift Dev Spaces** (and compatible CodeReady Workspaces). You can open the project in a browser-based IDE and run the full stack there.

- **Components:** One container for the APIs (UBI8 .NET 8.0) and one for the webapp (UBI8 Node.js 20). The devfile defines build and run steps.
- **Endpoints:** The APIs are exposed as public endpoints (e.g. api-customers, api-bills, api-raiders). The frontend is served and can call these APIs using the workspace URLs.
- **Flow:** Clone the repo into a Dev Spaces workspace, run the build step, then the run step; open the frontend URL and use the app as in the architecture diagram.

This allows developing and testing the centralized wallet in a cloud IDE without installing .NET or Node locally.
