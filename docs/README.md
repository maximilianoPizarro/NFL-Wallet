# NFL Stadium Wallet — Documentation (Jekyll)

This folder is a **Jekyll** site for **GitHub Pages**. It documents the NFL Stadium Wallet solution with Red Hat–inspired branding and uses the repo logo and favicon.

## Enable on GitHub Pages

1. In the repo: **Settings → Pages**.
2. Under **Build and deployment**, set **Source** to **Deploy from a branch**.
3. Choose the **main** (or your default) branch and **/docs** as the folder.
4. Save. The site will be available at `https://<user>.github.io/<repo>/` (or your custom domain).

## Build locally (optional)

```bash
cd docs
bundle install
bundle exec jekyll serve
```

Open `http://localhost:4000` (or the URL Jekyll prints). For a project site, set `baseurl` in `_config.yml` to your repo name (e.g. `baseurl: "/NFL-Wallet"`) so links work.

## Contents (all in English)

- **Home** — Purpose (centralized wallet for stadiums), overview, quick links.
- **Architecture** — Components, Podman Compose, Red Hat OpenShift Dev Spaces.
- **Deployment** — Helm chart install and main values.
- **Connectivity Link** — Gateway API, HTTPRoutes, hostnames, Kuadrant AuthPolicy and RateLimitPolicy, and diagram (connectivity link with auth and limit).
- **Security** — API keys and two policy types: Istio AuthorizationPolicy and AuthPolicy with Authorino (Connectivity Link).
- **Observability** — Cluster Observability Operator, gateway metrics, and applying `config/observability/`.
- **Publish to Artifact Hub** — Steps to package the Helm chart and add the repo to Artifact Hub.
