# Package the Helm chart and update the repo index in docs/ for GitHub Pages.
# Run from repo root: .\scripts\helm-package-docs.ps1
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ChartPath = Join-Path $RepoRoot "helm\nfl-wallet"
$DocsPath = Join-Path $RepoRoot "docs"
$RepoUrl = if ($env:HELM_REPO_URL) { $env:HELM_REPO_URL } else { "https://maximilianopizarro.github.io/NFL-Wallet" }

Set-Location $RepoRoot
helm package $ChartPath --destination $DocsPath
$IndexPath = Join-Path $DocsPath "index.yaml"
if (Test-Path $IndexPath) {
  helm repo index $DocsPath --url $RepoUrl --merge $IndexPath
} else {
  helm repo index $DocsPath --url $RepoUrl
}
Write-Host "Done. Chart packaged in $DocsPath. index.yaml updated."
Write-Host "Add and commit docs/*.tgz and docs/index.yaml, then push."
