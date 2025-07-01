# WikiQuiz Infrastructure (Ultra-Simplified)

This infrastructure setup is designed for **minimal cost** and **maximum simplicity**. It creates only the absolute essentials to run WikiQuiz.

## 🏗️ What Gets Created

- **Resource Group** - Container for all resources
- **PostgreSQL Flexible Server** - Cheapest database tier (Burstable B1ms)
- **Azure Container Instances** - Cheapest container hosting
- **Static Web App** - Free tier for frontend hosting

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

This workflow automatically:

- ✅ **Builds on every push** to main/develop branches
- ✅ **Tags images appropriately** (`:prd` for main, `:dev` for develop)
- ✅ **Uses GitHub secrets** for authentication
- ✅ **Only builds when backend changes**

### Step 5: Configure Secrets

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

## 📊 After Deployment

You'll get these outputs:

```bash
pulumi stack output
```

- **ApiUrl**: `http://20.XXX.XXX.XXX` (your backend API)
- **FrontendUrl**: `https://wikiquiz-frontend-dev-XXXXX.azurestaticapps.net` (your frontend)
- **DatabaseHost**: `wikiquiz-db-dev-XXXXXX.postgres.database.azure.com` (your database)

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

### View Container Logs

```bash
# Get resource group and container group names
pulumi stack output

# View logs in Azure CLI
az container logs --resource-group rg-wikiquiz-dev --name wikiquiz-api-dev
```

## 🧹 Cleanup

To destroy everything and stop costs:

```bash
pulumi destroy
```
