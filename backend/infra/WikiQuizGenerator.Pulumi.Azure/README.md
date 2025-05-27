# Azure Native C# Pulumi Project for Wiki Quiz

This directory contains a Pulumi project for deploying the Wiki Quiz infrastructure on Azure. It uses C# and Pulumi's Azure Native provider to provision all required resources in a modular, maintainable way.

## Infrastructure Overview

This project deploys the following Azure resources:

- **Resource Group**: Contains all other resources
- **User-Assigned Managed Identity**: Used by the Container App to securely access other resources
- **Log Analytics Workspace**: For application logging and monitoring
- **Azure Container Registry (ACR)**: Stores container images
- **Azure Key Vault**: Securely stores secrets
- **Azure App Configuration**: Stores application settings and Key Vault references
- **Azure Database for PostgreSQL Flexible Server**: Database for the application
- **Azure Container Apps Environment**: Hosts the Container App
- **Azure Container App**: Runs the application container

## Prerequisites

1. **Pulumi CLI**: [Install Pulumi](https://www.pulumi.com/docs/get-started/install/)
2. **Azure CLI**: [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
3. **.NET SDK**: Version 6.0 or later
4. **Azure Subscription**: You need an active Azure subscription

## Getting Started

### 1. Login to Azure and Pulumi

```bash
# Login to Azure
az login

# Login to Pulumi (if using Pulumi Cloud for state management)
pulumi login

# Alternatively, use local state management
pulumi login --local
```

### 2. Initialize the Pulumi Stack

```bash
# Navigate to the infrastructure directory
cd infra

# Create a new stack (e.g., dev, test, prod)
pulumi stack init dev
```

### 3. Configure the Stack

Pulumi uses configuration values to customize the deployment. Use the following commands to set required configuration values:

```bash
# Set the Azure region (defaults to centralus if not specified)
pulumi config set azure-native:location eastus

# Required configuration values
pulumi config set projectName wikiquizapi
pulumi config set environmentName Development  # Development, Test, or Production
pulumi config set containerImage acrwikiquizapii5yglprgz2pfq.azurecr.io/wikiquizapi:v0.1
pulumi config set containerPort 8080
pulumi config set postgresAdminLogin pgadmin
pulumi config set postgresDatabaseName WikiQuizGenerator
pulumi config set frontendUrl http://yoururl.com
pulumi config set jwtIssuer https://yoururl.com/
pulumi config set jwtAudience https://yoururl.net
```

### 4. Configure Secrets

Pulumi securely manages secrets by encrypting them at rest. Set the following secret values:

```bash
# Set secret values (these will be encrypted)
pulumi config set --secret postgresAdminPassword [YOUR_POSTGRES_PASSWORD]
pulumi config set --secret openAiApiKey [YOUR_OPENAI_API_KEY]
pulumi config set --secret googleClientId [YOUR_GOOGLE_CLIENT_ID]
pulumi config set --secret googleClientSecret [YOUR_GOOGLE_CLIENT_SECRET]
pulumi config set --secret jwtSecret [YOUR_JWT_SECRET]
```

### 5. Preview and Deploy

```bash
# Preview the deployment (no changes are made)
pulumi preview

# Deploy the infrastructure
pulumi up
```

Review the changes and confirm the deployment.

### 6. Access Resources

After deployment, Pulumi will output useful information about your resources, including:

- `containerAppUrl`: The fully qualified domain name for your Container App
- `acrLoginServer`: The login server for your Azure Container Registry
- `postgresFqdn`: The FQDN for your PostgreSQL server
- `keyVaultName`: The name of your Key Vault
- `keyVaultUri`: The URI of your Key Vault
- `appConfigStoreName`: The name of your App Configuration store
- `appConfigStoreEndpoint`: The endpoint URI of your App Configuration store

You can access these outputs at any time using:

```bash
pulumi stack output [OUTPUT_NAME]
```

## Managing Environment-Specific Configurations

You can create and manage multiple stacks for different environments:

```bash
# Create a production stack
pulumi stack init prod

# Select an existing stack
pulumi stack select dev

# List available stacks
pulumi stack ls
```

Each stack has its own configuration and state, allowing you to have different settings for different environments.

## Updating the Infrastructure

To update the infrastructure after changes to the code:

```bash
# Preview changes
pulumi preview

# Apply changes
pulumi up
```

## Destroying the Infrastructure

To tear down all resources:

```bash
pulumi destroy
```

**WARNING**: This will permanently delete all resources created by this Pulumi project. Use with caution.

## Project Structure

- **MyStack.cs**: The main stack definition that orchestrates all modules
- **Configuration.cs**: Strongly-typed configuration for the stack
- **Modules/**: Directory containing modular components
  - **IdentityModule.cs**: Manages User-Assigned Managed Identity
  - **MonitoringModule.cs**: Manages Log Analytics Workspace
  - **RegistryModule.cs**: Manages Azure Container Registry
  - **SecretsManagementModule.cs**: Manages Key Vault and its secrets
  - **AppConfigModule.cs**: Manages App Configuration and its key-values
  - **DatabaseModule.cs**: Manages PostgreSQL server and database
  - **ContainerAppsModule.cs**: Manages Container Apps Environment and Container App
  - **AuthorizationModule.cs**: Manages role assignments for the Managed Identity

## Best Practices

1. **Never commit secrets** to version control. Use `pulumi config set --secret` to manage sensitive values.
2. Use **separate stacks** for different environments (dev, test, prod).
3. Review changes carefully with `pulumi preview` before applying them.
4. Consider using [Pulumi CI/CD integration](https://www.pulumi.com/docs/guides/continuous-delivery/) for automated deployments.
5. After deployment, the app in the Container App will use Managed Identity to access secrets in Key Vault via App Configuration for maximum security.
