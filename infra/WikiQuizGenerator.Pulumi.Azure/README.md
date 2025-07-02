# WikiQuiz Infrastructure (Ultra Cost-Optimized & Scalable)

This infrastructure setup is designed for **absolute minimum cost** with **smart auto-scaling capabilities**. Every resource is configured for the cheapest possible tier with built-in scaling options.

## 🏗️ What Gets Created (Cost-Optimized)

- **Resource Group** - Container for all resources
- **PostgreSQL Flexible Server** - Burstable B1ms (1 vCore, 2GB RAM, auto-scaling storage)
- **Azure Container Instances** - Minimal CPU/RAM with burst limits for scaling
- **Static Web App** - Free tier for frontend hosting

## 💰 Cost Breakdown (Estimated Monthly)

### Development Environment

- **PostgreSQL Flexible Server B1ms**: ~$12-15/month
- **Container Instances (0.5 CPU, 1.5GB)**: ~$8-12/month
- **Static Web App**: FREE
- **Total**: ~$20-27/month

### Production Environment (Same specs)

- **Total**: ~$20-27/month per environment

## 🚀 Auto-Scaling Features

### PostgreSQL Database

- **Storage Auto-Grow**: Starts at 32GB, expands automatically
- **Burstable Performance**: CPU credits for handling traffic spikes
- **Backup**: 7-day retention (minimum cost)

### Container Instances

- **CPU Bursting**: 0.5 → 1.0 CPU when needed
- **Memory Bursting**: 1.5GB → 2.0GB when needed
- **Auto-Restart**: Automatic restart on failures
- **Health Checks**: Built-in liveness probes

## 🐳 Container Strategy

This setup uses **GitHub Container Registry (GHCR)** with environment-specific tags:

- **Development**: `ghcr.io/brittain9/wiki-quiz:dev`
- **Production**: `ghcr.io/brittain9/wiki-quiz:prd`

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
docker build -t ghcr.io/your-github-username/wiki-quiz:dev .
docker push ghcr.io/your-github-username/wiki-quiz:dev
```

#### Option B: Automated Build Pipeline (Recommended)

Create `.github/workflows/build-and-deploy.yml` in your repository root:

This workflow automatically:

- ✅ **Builds on every push** to main/develop branches
- ✅ **Tags images appropriately** (`:prd` for main, `:dev` for develop)
- ✅ **Uses GitHub secrets** for authentication
- ✅ **Only builds when backend changes**

### Step 5: Deploy Infrastructure

```bash
# Preview what will be created (should show 7+ resources including database)
pulumi preview

# Deploy everything
pulumi up

# Get your URLs and connection info
pulumi stack output
```

### Step 6: Setup Frontend Deployment

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

## 📊 After Deployment

You'll get these outputs:

```bash
pulumi stack output
```

- **ApiUrl**: `http://20.XXX.XXX.XXX` (your backend API)
- **FrontendUrl**: `https://wikiquiz-frontend-dev-XXXXX.azurestaticapps.net` (your frontend)
- **DatabaseHost**: `wikiquiz-db-dev-XXXXXX.postgres.database.azure.com` (your database)
- **DatabaseConnectionString**: Full connection string for manual access

## 🔧 Cost Optimization Features

### Automatic Cost Savings

1. **Burstable Database**: Only pay for CPU credits when you use them
2. **Minimal Storage**: Starts small, grows only when needed
3. **Single-Region Backup**: No geo-redundancy costs
4. **Container Bursting**: Scale up only during traffic spikes
5. **Health Monitoring**: Prevent unnecessary resource waste

### Manual Cost Controls

```bash
# Stop containers during downtime (dev environment)
az container stop --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev

# Restart when needed
az container start --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev

# Scale database down (if supported by Azure)
# Note: Burstable tier is already the cheapest option
```

## 🌍 Multiple Environments

### Development Environment

```bash
# Deploy to dev (uses Pulumi.dev.yaml)
pulumi stack select dev
pulumi config set githubUsername your-github-username
pulumi up

# Uses container: ghcr.io/your-github-username/wiki-quiz:dev
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

# Uses container: ghcr.io/your-github-username/wiki-quiz:prd
```

## 🔄 Deployment Workflow

### Automated (With GitHub Actions)

1. **Push to `develop` branch** → Builds `:dev` image → Deploy to dev environment
2. **Push to `main` branch** → Builds `:prd` image → Deploy to production environment

### Manual

```bash
# 1. Build and push new version
cd backend
docker build -t ghcr.io/your-username/wiki-quiz:dev .
docker push ghcr.io/your-username/wiki-quiz:dev

# 2. Deploy infrastructure update (if image tag is already correct, this is instant)
cd infra/WikiQuizGenerator.Pulumi.Azure
pulumi up

# 3. Restart container to pull new image
az container restart --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev
```

### Monitor Resources

```bash
# View container logs
az container logs --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev

# Check database connectivity
psql "$(pulumi stack output DatabaseConnectionString)"

# Monitor costs in Azure Portal
az account show --query tenantId
```

## 📈 Scaling Strategies

### Vertical Scaling (More Power)

If you need more performance, you can upgrade to higher tiers:

```bash
# Upgrade database to Standard_B2s (2 vCores, 4GB RAM)
# Update MyStack.cs and change sku Name to "Standard_B2s"
pulumi up

# Container scaling is automatic within burst limits
```

### Horizontal Scaling (More Instances)

For true horizontal scaling, consider upgrading to:

- **Azure Container Apps** (better auto-scaling)
- **Azure App Service** (multiple instances)
- **Azure Database for PostgreSQL** with read replicas

## 🧹 Cleanup

To destroy everything and stop costs:

```bash
pulumi destroy
```

## 🚨 Cost Alerts

Set up cost alerts in Azure Portal:

1. Go to **Cost Management + Billing**
2. Create budget alert for your resource group
3. Set threshold at $30/month for safety
