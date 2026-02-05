---
layout: default
title: Publish to Artifact Hub
description: "Steps to package the Helm chart and publish it to Artifact Hub."
---

## Publishing the Helm Chart to Artifact Hub

The `nfl-wallet` Helm chart includes [Artifact Hub annotations](https://artifacthub.io/docs/topics/annotations/helm/) in `Chart.yaml` (category, license, links, changelog). Follow these steps to publish the chart so it appears on [Artifact Hub](https://artifacthub.io).

### 1. Package the chart

From the **repository root**:

```bash
helm package helm/nfl-wallet --destination docs/
```

This creates `docs/nfl-wallet-<version>.tgz` (e.g. `docs/nfl-wallet-0.1.0.tgz`).

### 2. Update the Helm repository index (for GitHub Pages)

If you serve the chart from GitHub Pages as a Helm repo:

```bash
cd docs
helm repo index . --url https://maximilianopizarro.github.io/NFL-Wallet --merge index.yaml
cd ..
```

Commit and push `docs/nfl-wallet-*.tgz` and `docs/index.yaml` so the chart is available at the repo URL.

### 3. Add the repository to Artifact Hub

1. Go to [Artifact Hub](https://artifacthub.io) and sign in (e.g. with GitHub).
2. Click your **user/org** → **Add repository** (or **Control panel** → **Repositories** → **Add**).
3. Choose **Helm** as the repository kind.
4. Enter the **repository URL** (e.g. `https://maximilianopizarro.github.io/NFL-Wallet` for GitHub Pages).
5. Optional: set **Repository name** (e.g. `nfl-wallet`) and **Display name**.
6. Save. Artifact Hub will index the chart and list it.

### 4. Verify and share

- Search for `nfl-wallet` on Artifact Hub; the package page will show description, links, and install instructions.
- Users can add the repo and install:
  ```bash
  helm repo add nfl-wallet https://maximilianopizarro.github.io/NFL-Wallet
  helm repo update
  helm install nfl-wallet nfl-wallet/nfl-wallet -n nfl-wallet
  ```

### 5. Optional: Verified Publisher badge

For a **Verified Publisher** badge, link the Helm repository to a **verified** GitHub organization or user in Artifact Hub. Follow [Artifact Hub’s verification steps](https://artifacthub.io/docs/topics/repositories/#verified-publisher) (e.g. add a verification file or claim the repository from the control panel).

### Chart.yaml annotations (reference)

The chart already includes:

| Annotation | Purpose |
|------------|---------|
| `artifacthub.io/category` | Category (e.g. `integration-delivery`) |
| `artifacthub.io/license` | SPDX license (e.g. `Apache-2.0`) |
| `artifacthub.io/links` | Documentation and source URLs |
| `artifacthub.io/changes` | Changelog for the version |

You can add more (e.g. `artifacthub.io/images` for container images, `artifacthub.io/screenshots`) in `helm/nfl-wallet/Chart.yaml` as needed.
