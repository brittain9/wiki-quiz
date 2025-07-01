# WikiQuiz Infrastructure (Ultra-Simplified)

This infrastructure setup is designed for **minimal cost** and **maximum simplicity**. It creates only the absolute essentials to run WikiQuiz.

## 🏗️ What Gets Created

- **Resource Group** - Container for all resources
- **PostgreSQL Flexible Server** - Cheapest database tier (Burstable B1ms)
- **Azure Container Instances** - Cheapest container hosting
- **Static Web App** - Free tier for frontend hosting

**Estimated cost: ~$20-40/month** (vs $100-200+ for the original complex setup)

## 🐳 Container Strategy

This setup uses **GitHub Container Registry (GHCR)** with environment-specific tags:

- **Development**: `ghcr.io/your-username/wikiquiz-api:dev`
- **Production**: `ghcr.io/your-username/wikiquiz-api:prd`

## ⚙️ Configuration Note

**You don't need to edit any YAML files manually!**

- `Pulumi.yaml` = Project documentation (optional)
- `Pulumi.dev.yaml` & `Pulumi.prd.yaml` = Auto-created when you run `pulumi config set`

Just use the CLI commands below, and Pulumi handles the files automatically.

## 🚀 Complete Setup Guide

### Step 1: Prerequisites

```bash
# Install required tools
az login
pulumi login

# Navigate to this directory
cd infra/WikiQuizGenerator.Pulumi.Azure
```

### Step 2: Configure via CLI (No Manual File Editing!)

```bash
# Select dev environment
pulumi stack select dev

# Set your GitHub username (this determines your container registry URL)
pulumi config set githubUsername your-actual-github-username

# Set encrypted secrets (Pulumi encrypts these automatically)
pulumi config set --secret postgresPassword "YourSecurePassword123!"
pulumi config set --secret openAiApiKey "sk-your-openai-key-here"
pulumi config set --secret googleClientId "your-google-client-id"
pulumi config set --secret googleClientSecret "your-google-client-secret"
pulumi config set --secret jwtSecret "your-super-secret-jwt-key-32-chars-min"
```

### Step 3: Setup GitHub Container Registry

#### A. Generate GitHub Personal Access Token

1. Go to **GitHub** → **Settings** → **Developer settings** → **Personal access tokens** → **Tokens (classic)**
2. Click **"Generate new token (classic)"**
3. Select scopes:
   - ✅ `write:packages` (to push containers)
   - ✅ `read:packages` (to pull containers)
4. Copy the token (you'll need it for authentication)

#### B. Login to GitHub Container Registry

```bash
# Login to GHCR with your GitHub username and token
echo YOUR_GITHUB_TOKEN | docker login ghcr.io -u your-github-username --password-stdin
```

### Step 4: Build and Push Container Images

#### Option A: Manual Build (Quick Start)

```bash
# From project root
cd backend

# Build and tag for development
docker build -t ghcr.io/your-github-username/wikiquiz-api:dev .
docker push ghcr.io/your-github-username/wikiquiz-api:dev

# Build and tag for production
docker build -t ghcr.io/your-github-username/wikiquiz-api:prd .
docker push ghcr.io/your-github-username/wikiquiz-api:prd
```

#### Option B: Automated Build Pipeline (Recommended)

Create `.github/workflows/build-and-deploy.yml` in your repository root:

```yaml
name: Build and Deploy WikiQuiz

on:
  push:
    branches: [main, develop]
    paths: ["backend/**"]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/wikiquiz-api

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Log in to Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch,enable={{is_default_branch}},suffix=-prd
            type=ref,event=branch,enable={{!is_default_branch}},suffix=-dev
            type=ref,event=pr,suffix=-pr{{number}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v5
        with:
          context: ./backend
          file: ./backend/src/WikiQuizGenerator.Api/Dockerfile
          push: true
          tags: |
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.ref == 'refs/heads/main' && 'prd' || 'dev' }}
          labels: ${{ steps.meta.outputs.labels }}
```

This workflow automatically:

- ✅ **Builds on every push** to main/develop branches
- ✅ **Tags images appropriately** (`:prd` for main, `:dev` for develop)
- ✅ **Uses GitHub secrets** for authentication
- ✅ **Only builds when backend changes**

### Step 5: Configure Secrets

You have **two options** for managing secrets:

#### Option A: Environment File (Development Only) 🔧

Create a `.env` file in the infrastructure directory:

```bash
# Copy the example file and edit it
cp env.example .env

# Edit .env with your actual values
# (Use your favorite editor - nano, vim, VS Code, etc.)
nano .env
```

Or create it manually:

```bash
# Create .env file (DO NOT commit this to git!)
cat > .env << 'EOF'
POSTGRES_PASSWORD=YourSecurePassword123!
OPENAI_API_KEY=sk-your-openai-api-key-here
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
JWT_SECRET=your-super-secret-jwt-key-min-32-chars
EOF
```

Then set the secrets from the env file:

```bash
# Load secrets from .env file
source .env
pulumi config set --secret postgresPassword "$POSTGRES_PASSWORD"
pulumi config set --secret openAiApiKey "$OPENAI_API_KEY"
pulumi config set --secret googleClientId "$GOOGLE_CLIENT_ID"
pulumi config set --secret googleClientSecret "$GOOGLE_CLIENT_SECRET"
pulumi config set --secret jwtSecret "$JWT_SECRET"
```

#### Option B: Direct Pulumi Secrets (Recommended for Production) 🔒

Set secrets directly (Pulumi encrypts them):

```bash
# Pulumi will prompt for each value and encrypt it
pulumi config set --secret postgresPassword
pulumi config set --secret openAiApiKey
pulumi config set --secret googleClientId
pulumi config set --secret googleClientSecret
pulumi config set --secret jwtSecret
```

### Step 6: Deploy

```bash
# Preview what will be created (should show 4 resources)
pulumi preview

# Deploy everything
pulumi up

# Get your URLs
pulumi stack output
```

### Step 7: Setup Frontend Deployment

After the infrastructure is deployed, you need to connect your frontend to the Static Web App:

#### Option A: GitHub Integration (Recommended)

1. Go to **Azure Portal** → **Resource Groups** → **rg-wikiquiz-dev** → **wikiquiz-frontend-dev**
2. Click **"Manage deployment"** → **"GitHub"**
3. Connect your GitHub repository
4. Configure build settings:
   - **Source**: GitHub
   - **Repository**: your-username/wiki-quiz
   - **Branch**: main (or your default branch)
   - **Build presets**: Custom
   - **App location**: `/frontend`
   - **Output location**: `dist`

#### Option B: Manual Upload

```bash
# Build the frontend
cd frontend
npm install
npm run build

# The dist/ folder contains your built frontend
# Upload it manually via Azure Portal → Static Web App → "Browse" → drag & drop
```

## 📊 After Deployment

You'll get these outputs:

```bash
pulumi stack output
```

- **ApiUrl**: `http://20.XXX.XXX.XXX` (your backend API)
- **FrontendUrl**: `https://wikiquiz-frontend-dev-XXXXX.azurestaticapps.net` (your frontend)
- **DatabaseHost**: `wikiquiz-db-dev-XXXXXX.postgres.database.azure.com` (your database)

## 🔧 How Configuration Works Now

**Before** (Complex): Key Vault → App Configuration → Container Apps
**After** (Simple): Environment Variables → Container Instances

Your backend app receives these environment variables:

```bash
ASPNETCORE_ENVIRONMENT=dev
SKIP_APP_CONFIG=true
ConnectionStrings__DefaultConnection=Host=...;Database=wikiquiz;...
OpenAI__ApiKey=sk-...
GoogleAuth__ClientId=...
GoogleAuth__ClientSecret=...
JwtOptions__Secret=...
WikiQuizApp__FrontendUri=https://your-static-web-app-url
FORWARDEDHEADERS_ENABLED=true
```

## 🌍 Multiple Environments

### Development Environment

```bash
# Deploy to dev (uses Pulumi.dev.yaml)
pulumi stack select dev
pulumi config set githubUsername your-github-username
pulumi up

# Uses container: ghcr.io/your-github-username/wikiquiz-api:dev
```

### Production Environment

```bash
# Deploy to production (uses Pulumi.prd.yaml)
pulumi stack select prd
pulumi config set githubUsername your-github-username

# Set production secrets
pulumi config set --secret postgresPassword [prod-password]
pulumi config set --secret openAiApiKey [prod-openai-key]
# ... etc

pulumi up

# Uses container: ghcr.io/your-github-username/wikiquiz-api:prd
```

## 🔄 Deployment Workflow

### Automated (With GitHub Actions)

1. **Push to `develop` branch** → Builds `:dev` image → Deploy to dev environment
2. **Push to `main` branch** → Builds `:prd` image → Deploy to production environment

### Manual

```bash
# 1. Build and push new version
cd backend
docker build -t ghcr.io/your-username/wikiquiz-api:dev .
docker push ghcr.io/your-username/wikiquiz-api:dev

# 2. Deploy infrastructure update (if image tag is already correct, this is instant)
cd infra/WikiQuizGenerator.Pulumi.Azure
pulumi up

# 3. Restart container to pull new image
az container restart --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev
```

## 💰 Cost Breakdown

- **PostgreSQL Flexible Server (B1ms)**: ~$12-15/month
- **Container Instances (0.5 vCPU, 1GB)**: ~$8-12/month
- **Static Web App (Free tier)**: $0/month
- **GitHub Container Registry**: $0/month (500MB bandwidth free)
- **Resource Group**: $0/month

**Total: ~$20-27/month**

## 🔍 Monitoring and Troubleshooting

### View Container Logs

```bash
# Get resource group and container group names
pulumi stack output

# View logs in Azure CLI
az container logs --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev
```

### Test Your API

```bash
# Get the API URL
API_URL=$(pulumi stack output ApiUrl)

# Test health endpoint
curl $API_URL/health

# Test a quiz endpoint (if you have any)
curl $API_URL/api/quiz
```

### Check Container Registry

```bash
# View your published images
docker run --rm -it \
  -v ~/.docker/config.json:/root/.docker/config.json:ro \
  ghcr.io/distribution/distribution:latest \
  registry catalog ghcr.io/your-username
```

### Database Connection

```bash
# Get database host
DB_HOST=$(pulumi stack output DatabaseHost)

# Connect with psql (if you have it installed)
psql "host=$DB_HOST user=wikiquizadmin dbname=wikiquiz sslmode=require"
```

## 🔒 GitHub Container Registry Permissions

Your container images are **private by default** in GHCR. To make them publicly accessible (optional):

1. Go to **GitHub** → **Your Repository** → **Packages**
2. Click on your `wikiquiz-api` package
3. **Package settings** → **Change visibility** → **Public**

Or keep them private (recommended) and Azure will authenticate using the container registry credentials.

## 🚨 Important Security Notes

1. **Never commit `.env` files** to version control
2. **Use Option B (Pulumi secrets)** for production
3. **Your secrets are encrypted** in Pulumi state files
4. **GitHub tokens** should have minimal required scopes
5. **Container images** are private by default in GHCR

## 🔄 Updates and Maintenance

### Update Container Image

With the automated pipeline, simply:

```bash
# For development
git push origin develop

# For production
git push origin main
```

### Manual Container Update

```bash
# Build new version
cd backend
docker build -t ghcr.io/your-username/wikiquiz-api:dev .
docker push ghcr.io/your-username/wikiquiz-api:dev

# Restart the container to pull new image
az container restart --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev
```

### Update Secrets

```bash
# Change a secret
pulumi config set --secret openAiApiKey [new-key]
pulumi up
```

## 🧹 Cleanup

To destroy everything and stop costs:

```bash
pulumi destroy
```

**Warning**: This permanently deletes all resources and data!

## 🆘 Common Issues

### Container Won't Start

1. Check container logs: `az container logs --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev`
2. Verify your container image exists: `docker pull ghcr.io/your-username/wikiquiz-api:dev`
3. Check environment variables in Azure Portal

### Docker Authentication Failed

1. Regenerate GitHub token with correct scopes
2. Re-login: `echo TOKEN | docker login ghcr.io -u username --password-stdin`
3. Try pulling image locally first

### Image Not Found

1. Check GitHub username in config: `pulumi config get githubUsername`
2. Verify image exists: Visit `https://github.com/users/your-username/packages`
3. Ensure image is tagged correctly (`:dev` or `:prd`)

### GitHub Actions Build Failed

1. Check that `GITHUB_TOKEN` has package write permissions
2. Verify the Dockerfile path in the workflow
3. Check that the workflow triggers on the right branches

### "SKIP_APP_CONFIG" Environment Variable

Your app should have this line in `Program.cs`:

```csharp
bool skipAppConfig = string.Equals(
    Environment.GetEnvironmentVariable("SKIP_APP_CONFIG"),
    "true",
    StringComparison.OrdinalIgnoreCase);
```

This bypasses the complex Azure App Configuration setup.
