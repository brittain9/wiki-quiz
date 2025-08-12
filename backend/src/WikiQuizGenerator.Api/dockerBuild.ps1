param(
  [switch]$SkipLogin
)

# Variables
$OWNER = "brittain9"
$REPO = "wiki-quiz"
$IMAGE = "ghcr.io/$OWNER/$REPO"
$COMMIT_TAG = (git rev-parse --short HEAD)
$PRD_TAG = "prd"

# Resolve repo root relative to this script
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "../../..")
$Dockerfile = Join-Path $RepoRoot "backend/src/WikiQuizGenerator.Api/Dockerfile"
$ContextDir = Join-Path $RepoRoot "backend/src"

if (-not (Test-Path $Dockerfile)) { throw "Dockerfile not found: $Dockerfile" }
if (-not (Test-Path $ContextDir)) { throw "Build context not found: $ContextDir" }

# Login to GHCR (PAT in $env:GHCR_TOKEN)
if (-not $SkipLogin) {
  if (-not $env:GHCR_TOKEN) { throw "Please set GHCR_TOKEN environment variable to a GHCR PAT with write:packages scope." }
  [System.Text.Encoding]::UTF8.GetBytes($env:GHCR_TOKEN) | docker login ghcr.io -u $OWNER --password-stdin
}

# Build (Linux image for Azure). Add --platform if your Docker is not using Linux engine.
docker build `
  -f $Dockerfile `
  -t "$IMAGE:latest" `
  -t "$IMAGE:$COMMIT_TAG" `
  -t "$IMAGE:$PRD_TAG" `
  --platform linux/amd64 `
  $ContextDir

# Optional: test locally
# docker run --rm -p 8080:8080 $IMAGE:$PRD_TAG

# Push all tags (Pulumi references :prd)
docker push $IMAGE --all-tags