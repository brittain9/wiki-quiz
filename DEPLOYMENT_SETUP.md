# Deployment Setup Guide

This guide explains how to set up automatic deployment for both the frontend (Azure Static Web App) and backend (Azure Container Apps) components of the Wiki Quiz application.

## Overview

- **Frontend**: Deployed to Azure Static Web App, automatically builds and deploys when changes are pushed to the `main` branch in the `frontend/` directory
- **Backend**: Deployed to Azure Container Apps, automatically builds Docker image and deploys when changes are pushed to the `main` branch in the `backend/` directory

## Prerequisites

1. Azure CLI installed and logged in
2. Pulumi CLI installed
3. GitHub repository with appropriate permissions
4. Azure subscription with necessary permissions

## Setup Steps

### 1. GitHub Token for Static Web App

Create a GitHub Personal Access Token with the following permissions:
- `repo` (Full control of private repositories)
- `workflow` (Update GitHub Action workflows)

Set this token in your Pulumi configuration:
```bash
cd infra/WikiQuizGenerator.Pulumi.Azure
pulumi config set --secret wikiquiz:githubToken <your-github-token>
```

### 2. Deploy Infrastructure

Deploy the updated infrastructure to create the Static Web App:
```bash
cd infra/WikiQuizGenerator.Pulumi.Azure
pulumi up
```

This will create:
- Azure Static Web App for frontend hosting
- Update the existing Container App configuration

### 3. Configure GitHub Secrets

After deploying the infrastructure, you need to configure several GitHub secrets:

#### For Static Web App Deployment
1. Go to your GitHub repository → Settings → Secrets and variables → Actions
2. Add the following secret:
   - `AZURE_STATIC_WEB_APPS_API_TOKEN`: Get this from the Azure portal
     - Navigate to your Static Web App resource
     - Go to "Manage deployment token"
     - Copy the deployment token

#### For Container App Deployment
Add the following secret:
- `AZURE_CREDENTIALS`: Azure service principal credentials in JSON format
  
To create the service principal:
```bash
az ad sp create-for-rbac --name "github-actions-wikiquiz" \
  --role contributor \
  --scopes /subscriptions/<your-subscription-id>/resourceGroups/rg-wikiquiz-prd \
  --sdk-auth
```

Copy the entire JSON output and paste it as the `AZURE_CREDENTIALS` secret value.

### 4. Update Frontend Configuration

Ensure your frontend is configured to use the correct API URL. Update your frontend configuration to point to the Container App URL (available as output from Pulumi deployment).

### 5. Trigger First Deployment

#### Frontend
The Static Web App will be automatically configured with a GitHub Actions workflow. To trigger the first deployment:
1. Make any small change to a file in the `frontend/` directory
2. Commit and push to the `main` branch
3. The GitHub Action will automatically build and deploy your frontend

#### Backend
To trigger the first backend deployment:
1. Make any small change to a file in the `backend/` directory
2. Commit and push to the `main` branch
3. The GitHub Action will build the Docker image and deploy it to Container Apps

## Workflow Details

### Frontend Workflow (`.github/workflows/frontend-deploy.yml`)
- **Triggers**: Push to `main` branch with changes in `frontend/` directory
- **Actions**: 
  - Builds the React/Vite application
  - Deploys to Azure Static Web App
  - Handles PR previews and cleanup

### Backend Workflow (`.github/workflows/backend-deploy.yml`)
- **Triggers**: Push to `main` branch with changes in `backend/` directory
- **Actions**:
  - Builds Docker image
  - Pushes to GitHub Container Registry (ghcr.io)
  - Updates Azure Container App with new image

## Monitoring and Troubleshooting

### Check Deployment Status
- **Frontend**: Check the Actions tab in your GitHub repository
- **Backend**: Check both GitHub Actions and Azure Container Apps logs in the Azure portal

### Common Issues

1. **Static Web App deployment fails**:
   - Verify the `AZURE_STATIC_WEB_APPS_API_TOKEN` secret is correct
   - Check that the app location and output location are correct in the workflow

2. **Container App deployment fails**:
   - Verify `AZURE_CREDENTIALS` secret is correct and has proper permissions
   - Check Docker build logs in GitHub Actions
   - Verify Container Registry permissions

3. **Build failures**:
   - Check that your frontend builds successfully locally with `npm run build`
   - Verify Docker image builds locally in the backend directory

## Environment Variables

The backend Container App is automatically configured with:
- Cosmos DB connection strings
- Application Insights configuration
- OAuth client secrets
- JWT configuration
- API endpoints

The frontend Static Web App will need to be configured with:
- API endpoint URL (from Container App)
- OAuth client IDs (public values)

## Security Notes

- All sensitive configuration is stored as Azure Key Vault secrets or Pulumi encrypted config
- GitHub secrets are encrypted and only accessible to workflows
- Container images are stored privately in GitHub Container Registry
- Static Web App deployment tokens have limited scope

## Next Steps

1. Configure any environment-specific settings
2. Set up monitoring and alerting
3. Configure custom domains if needed
4. Set up staging environments using the same pattern
