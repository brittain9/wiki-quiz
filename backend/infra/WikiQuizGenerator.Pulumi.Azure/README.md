# WikiQuiz Azure Infrastructure (Pulumi)

This directory contains the Pulumi infrastructure-as-code for deploying the WikiQuiz application to Azure. The infrastructure supports multiple environments with automatic custom domain configuration.

## 🏗️ Architecture Overview

The infrastructure deploys the following Azure resources:

- **Resource Group**: Container for all resources
- **User Assigned Identity**: For secure service-to-service authentication
- **Log Analytics Workspace**: Centralized logging and monitoring
- **Azure Container Registry**: Private container image storage
- **PostgreSQL Flexible Server**: Managed database with environment-specific sizing
- **Key Vault**: Secure secret storage with RBAC
- **App Configuration**: Centralized configuration management
- **Container Apps Environment**: Serverless container hosting platform
- **Container App**: The main application with auto-scaling
- **Managed Certificate**: Automatic SSL certificate management for custom domains

## 🌍 Environment Configuration

### Environment-Specific Resources

| Environment     | Location   | Database SKU          | Container Resources | Replicas | Custom Domain                       |
| --------------- | ---------- | --------------------- | ------------------- | -------- | ----------------------------------- |
| **Development** | Central US | B1ms (1 vCore, 2GB)   | 0.25 CPU, 0.5GB RAM | 0-1      | Disabled                            |
| **Test**        | Central US | B2s (2 vCore, 4GB)    | 0.5 CPU, 1GB RAM    | 1-3      | api-test.quiz.alexanderbrittain.com |
| **Production**  | East US    | D2s_v3 (2 vCore, 8GB) | 1.0 CPU, 2GB RAM    | 2-10     | api.quiz.alexanderbrittain.com      |

### Custom Domain Configuration

The infrastructure supports automatic custom domain setup with managed SSL certificates:

- **Development**: Custom domains disabled (uses default Container App URL)
- **Test**: `api-test.quiz.alexanderbrittain.com`
- **Production**: `api.quiz.alexanderbrittain.com`

## 🚀 Quick Start

### Prerequisites

1. **Azure CLI** - [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **Pulumi CLI** - [Install Pulumi](https://www.pulumi.com/docs/get-started/install/)
3. **PowerShell** - For deployment scripts
4. **Azure Subscription** - With appropriate permissions

### Initial Setup

1. **Clone and navigate to the infrastructure directory:**

   ```bash
   cd infra/WikiQuizGenerator.Pulumi.Azure
   ```

2. **Login to Azure and Pulumi:**

   ```bash
   az login
   pulumi login
   ```

3. **Initialize Pulumi stack for your environment:**

   ```bash
   # For development
   pulumi stack init dev

   # For test
   pulumi stack init tst

   # For production
   pulumi stack init prd
   ```

### Deployment

Use the automated deployment script:

```powershell
# Deploy to development
./deploy.ps1 -Environment dev

# Deploy to test
./deploy.ps1 -Environment tst

# Deploy to production
./deploy.ps1 -Environment prd
```

The script will:

- Validate prerequisites
- Prompt for required secrets
- Set configuration values
- Deploy the infrastructure
- Display connection information

## 🌐 Custom Domain Setup

### Automatic Custom Domain Configuration

The infrastructure automatically configures custom domains for test and production environments.

### Manual DNS Setup Process (Cloudflare)

1. **Deploy the infrastructure first:**

   ```powershell
   ./deploy.ps1 -Environment tst
   ```

2. **Get the Container App URL:**

   ```bash
   pulumi stack output ContainerAppUrl
   # Example output: https://ca-wikiquiz-test.kindpond-12345678.centralus.azurecontainerapps.io
   ```

3. **Create DNS records in Cloudflare:**

   **CNAME Record:**

   - **Type**: CNAME
   - **Name**: `api-test` (for api-test.quiz.alexanderbrittain.com)
   - **Target**: `ca-wikiquiz-test.kindpond-12345678.centralus.azurecontainerapps.io` (from step 2, without https://)
   - **TTL**: Auto or 300 seconds
   - **Proxy status**: DNS only (gray cloud, not proxied)

4. **Get the domain verification TXT record:**

   - Go to Azure Portal → Container Apps → Your Container App → Custom domains
   - Click "Add custom domain"
   - Enter your domain (e.g., `api-test.quiz.alexanderbrittain.com`)
   - Azure will show you the required TXT record

5. **Create the TXT record in Cloudflare:**

   - **Type**: TXT
   - **Name**: `asuid.api-test` (for the verification subdomain)
   - **Content**: [The verification string from Azure portal]
   - **TTL**: Auto or 300 seconds

6. **Complete domain verification in Azure:**
   - Return to the Azure portal custom domain setup
   - Click "Validate" to verify the TXT record
   - Add the domain once validation passes

### Custom Domain Configuration Options

You can customize domains in the environment configuration files:

```yaml
# Pulumi.tst.yaml
config:
  enableCustomDomain: true
  customDomain: api-test.quiz.alexanderbrittain.com
```

### Important Notes for Cloudflare

- **Disable Proxy**: Make sure the CNAME record is "DNS only" (gray cloud), not proxied through Cloudflare
- **SSL/TLS Mode**: Set to "Full" or "Full (strict)" in Cloudflare SSL/TLS settings
- **DNS Propagation**: Usually takes 5-10 minutes with Cloudflare
- **Certificate**: Azure will automatically provision a managed SSL certificate

## 🔧 Configuration Management

### Environment Files

- `Pulumi.dev.yaml` - Development configuration
- `Pulumi.tst.yaml` - Test configuration
- `Pulumi.prd.yaml` - Production configuration

### Required Secrets

Set these secrets for each environment:

```bash
pulumi config set --secret postgresAdminPassword "YourSecurePassword123!"
pulumi config set --secret openAiApiKey "sk-your-openai-api-key"
pulumi config set --secret googleClientId "your-google-client-id"
pulumi config set --secret googleClientSecret "your-google-client-secret"
pulumi config set --secret jwtSecret "your-jwt-secret-key"
```

### JWT Configuration

The infrastructure automatically configures JWT settings:

- **Issuer**: Uses custom domain URL when enabled, falls back to environment-specific defaults
- **Audience**: Environment-specific API identifiers (`wikiquiz-api-dev`, `wikiquiz-api-test`, `wikiquiz-api-prod`)

## 📊 Container Image Management

### Image Configuration

Configure container images in environment files:

```yaml
config:
  containerImageName: wikiquizapi
  containerImageTag: v0.1 # dev
  # containerImageTag: latest  # test
  # containerImageTag: stable  # production
```

### Building and Pushing Images

```bash
# Build and tag image
docker build -t wikiquizapi:v0.1 .

# Tag for ACR
docker tag wikiquizapi:v0.1 [ACR_LOGIN_SERVER]/wikiquizapi:v0.1

# Push to ACR
az acr login --name [ACR_NAME]
docker push [ACR_LOGIN_SERVER]/wikiquizapi:v0.1
```

## 🔍 Monitoring and Troubleshooting

### View Stack Outputs

```bash
pulumi stack output
```

### Check Resource Status

```bash
# List all resources
pulumi stack --show-urns

# View specific resource
az containerapp show --name [CONTAINER_APP_NAME] --resource-group [RG_NAME]
```

### Common Issues

1. **Custom Domain Validation Failing**

   - Verify DNS records are correctly configured
   - Check TXT record value in Azure portal
   - Wait for DNS propagation (up to 48 hours)

2. **Container App Not Starting**

   - Check container logs in Azure portal
   - Verify image exists in ACR
   - Check environment variables and secrets

3. **Database Connection Issues**
   - Verify PostgreSQL server is running
   - Check connection string in Key Vault
   - Ensure firewall rules allow Container App access

### Useful Commands

```bash
# View container app logs
az containerapp logs show --name [CONTAINER_APP_NAME] --resource-group [RG_NAME]

# Check custom domain status
az containerapp hostname list --name [CONTAINER_APP_NAME] --resource-group [RG_NAME]

# Verify certificate status
az containerapp env certificate list --name [ENVIRONMENT_NAME] --resource-group [RG_NAME]
```

## 🔄 CI/CD Integration

The infrastructure supports automated deployments through GitHub Actions or Azure DevOps:

1. Store Pulumi access token as secret
2. Configure Azure service principal
3. Set environment-specific secrets
4. Use deployment script in pipeline

## 🧹 Cleanup

To destroy the infrastructure:

```bash
pulumi destroy
```

**Warning**: This will permanently delete all resources and data.

## 📚 Additional Resources

- [Pulumi Azure Native Documentation](https://www.pulumi.com/registry/packages/azure-native/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Container Apps Custom Domains](https://docs.microsoft.com/en-us/azure/container-apps/custom-domains-certificates)
