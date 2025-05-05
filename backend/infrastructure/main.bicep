@description('The base name for resources, unique across Azure.')
@minLength(3)
@maxLength(12)
param projectName string

@description('The Azure region where resources will be deployed.')
param location string = resourceGroup().location

@description('The environment name (e.g., Development, Production). Used for resource naming and App Config labels.')
@allowed([
  'Development'
  'Production'
  'Test'
])
param environmentName string = 'Development'

// Add a short environment name for resource naming
var environmentShort = environmentName == 'Production' ? 'prod' : environmentName == 'Development' ? 'dev' :  environmentName == 'Test' ? 'test' : toLower(environmentName)

@description('The image for the Web API (e.g., "myrepo/myapi:latest").')
param containerImage string

@description('The port your container listens on.')
param containerPort int = 8080

@description('The administrator login username for the PostgreSQL server.')
param postgresAdminLogin string

@description('The administrator login password for the PostgreSQL server. Will be stored in Key Vault.')
@secure()
param postgresAdminPassword string

@description('The name of the initial database to create on the PostgreSQL server.')
param postgresDatabaseName string = 'WikiQuizGenerator'

@description('The OpenAI API Key. Will be stored in Key Vault.')
@secure()
param openAiApiKey string

@description('The Google OAuth Client ID. Will be stored in Key Vault.')
@secure()
param googleClientId string

@description('The Google OAuth Client Secret. Will be stored in Key Vault.')
@secure()
param googleClientSecret string

@description('The JWT Issuer URI. Will be stored in App Config.')
param jwtIssuer string

@description('The JWT Audience URI. Will be stored in App Config.')
param jwtAudience string

@description('The JWT Secret key. Will be stored in Key Vault.')
@secure()
param jwtSecret string

// --- Variables ---
var uniqueSuffix = uniqueString(resourceGroup().id, projectName, location) // Ensures uniqueness for globally unique resources

// Add managed identity name
var managedIdentityRaw = 'id-${projectName}-${environmentShort}'
var managedIdentityName = toLower(substring(managedIdentityRaw, 0, min(24, length(managedIdentityRaw))))

// Resource name variables with prefix at the beginning and length limits (safe substring)
// Key Vault: 3-24 chars, alphanumeric, start/end with letter/digit - Restoring environmentShort
var keyVaultRaw = 'kv${projectName}${environmentShort}${uniqueSuffix}'
var keyVaultName = toLower(substring(keyVaultRaw, 0, min(24, length(keyVaultRaw))))
// Container Registry: 5-50 chars, alphanumeric, no hyphens, must be unique - REMOVED environmentShort
var containerRegistryRaw = 'acr${projectName}${uniqueSuffix}'
var containerRegistryName = toLower(substring(containerRegistryRaw, 0, min(50, length(containerRegistryRaw))))
// App Config: 5-50 chars, alphanumeric and hyphens, start/end with alphanumeric
var appConfigStoreRaw = 'acfg-${projectName}-${environmentShort}-${uniqueSuffix}'
var appConfigStoreName = toLower(substring(appConfigStoreRaw, 0, min(50, length(appConfigStoreRaw))))
// Log Analytics: 4-63 chars, letters, numbers, and hyphens, start/end with letter/number
var logAnalyticsWorkspaceRaw = 'log-${projectName}-${uniqueSuffix}'
var logAnalyticsWorkspaceName = toLower(substring(logAnalyticsWorkspaceRaw, 0, min(63, length(logAnalyticsWorkspaceRaw))))
// Postgres Server: 3-63 chars, lowercase letters, numbers, and hyphens, start/end with letter/number
var postgresServerRaw = 'pg-${projectName}-${environmentShort}-${uniqueSuffix}'
var postgresServerName = toLower(substring(postgresServerRaw, 0, min(63, length(postgresServerRaw))))
// Container App Env: 1-32 chars, alphanumeric and hyphens, start/end with alphanumeric
var containerAppEnvRaw = 'cae-${projectName}-${environmentShort}'
var containerAppEnvName = toLower(substring(containerAppEnvRaw, 0, min(32, length(containerAppEnvRaw))))
// Container App: 1-32 chars, alphanumeric and hyphens, start/end with alphanumeric
var containerAppRaw = 'app-${projectName}-${environmentShort}'
var containerAppName = toLower(substring(containerAppRaw, 0, min(32, length(containerAppRaw))))

// Determine the ASP.NET Core environment name based on the deployment environment parameter
// Now directly use the parameter as its allowed values match ASP.NET Core expectations
var aspNetCoreEnvironmentName = environmentName

// Key names expected by the C# application via IConfiguration
var configKeyPrefix = 'wikiquizapp:'
var configKeyOpenAIApiKey = '${configKeyPrefix}OpenAIApiKey' // Secret - will be KV reference
var configKeyGoogleClientId = '${configKeyPrefix}AuthGoogleClientID' // Secret - will be KV reference
var configKeyGoogleClientSecret = '${configKeyPrefix}AuthGoogleClientSecret' // Secret - will be KV reference
var configKeyPostgresConnectionString = '${configKeyPrefix}ConnectionString'

// JWT Configuration Keys
var configKeyJwtIssuer = 'JwtOptions:Issuer'
var configKeyJwtAudience = 'JwtOptions:Audience'
var configKeyJwtSecret = 'JwtOptions:Secret'

// Names for secrets stored in Key Vault
var secretNameOpenAIApiKey = 'OpenAIApiKey'
var secretNameGoogleClientId = 'GoogleClientId'
var secretNameGoogleClientSecret = 'GoogleClientSecret'
var secretNameJwtSecret = 'JwtSecret'
var secretNamePostgresConnectionString = 'PostgresConnectionString'

// --- Resource Definitions ---

// 0. User-Assigned Managed Identity (Create this first)
@description('Creates a user-assigned managed identity for the Container App')
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// 1. Log Analytics Workspace (Required for Container Apps Environment)
@description('Creates the Log Analytics Workspace.')
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// 2. Azure Container Registry (Basic Tier)
@description('Creates the Azure Container Registry.')
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false // Use Managed Identity
  }
}

// 3. Azure Key Vault (Standard Tier)
@description('Creates the Azure Key Vault to store secrets.')
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
  }
}

// 3.2 Store OpenAI API Key in Key Vault
@description('Stores the OpenAI API Key as a secret in Key Vault.')
resource openAiApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretNameOpenAIApiKey // Use variable
  properties: {
    value: openAiApiKey
  }
}

// 3.3 Store Google Client ID in Key Vault
@description('Stores the Google Client ID as a secret in Key Vault.')
resource googleClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretNameGoogleClientId // Use variable
  properties: {
    value: googleClientId
  }
}

// 3.4 Store Google Client Secret in Key Vault
@description('Stores the Google Client Secret as a secret in Key Vault.')
resource googleClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretNameGoogleClientSecret // Use variable
  properties: {
    value: googleClientSecret
  }
}

// 3.5 Store JWT Secret in Key Vault
@description('Stores the JWT Secret as a secret in Key Vault.')
resource jwtSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretNameJwtSecret // Use variable
  properties: {
    value: jwtSecret
  }
}

// 3.6 Store PostgreSQL Connection String in Key Vault
@description('Stores the PostgreSQL connection string as a secret in Key Vault.')
resource postgresConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretNamePostgresConnectionString
  properties: {
    value: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${postgresDatabase.name};Username=${postgresAdminLogin};Password=${postgresAdminPassword}'
  }
  dependsOn: [postgresServer, postgresDatabase]
}


// 4. Azure App Configuration
@description('Creates the Azure App Configuration store using free tier for development.')
resource appConfigStore 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigStoreName
  location: location
  sku: {
    name: 'free'
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// 4.1 Add Key-Values to App Configuration (including Key Vault References)
// These values will be loaded by the C# application via IConfiguration

// --- Non-Secret Values ---

// JWT Configuration - Non-Secret Values
@description('Adds JWT Issuer to App Configuration.')
resource appConfigJwtIssuer 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyJwtIssuer
  properties: {
    value: jwtIssuer
    label: aspNetCoreEnvironmentName
  }
}

@description('Adds JWT Audience to App Configuration.')
resource appConfigJwtAudience 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyJwtAudience
  properties: {
    value: jwtAudience
    label: aspNetCoreEnvironmentName
  }
}

// --- Secret Values (Key Vault References) ---
@description('Adds OpenAI API Key (as Key Vault Reference) to App Configuration.')
resource appConfigOpenAIApiKey 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyOpenAIApiKey // Key matches C# expectation
  properties: {
    value: '{"uri":"${keyVault.properties.vaultUri}secrets/${openAiApiKeySecret.name}"}'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    label: aspNetCoreEnvironmentName // Uses the environmentName parameter directly now
  }
  dependsOn: [ keyVault, openAiApiKeySecret ]
}

@description('Adds Google Client ID (as Key Vault Reference) to App Configuration.')
resource appConfigGoogleClientId 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyGoogleClientId // Key matches C# expectation
  properties: {
    value: '{"uri":"${keyVault.properties.vaultUri}secrets/${googleClientIdSecret.name}"}'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    label: aspNetCoreEnvironmentName // Uses the environmentName parameter directly now
  }
  dependsOn: [ keyVault, googleClientIdSecret ]
}

@description('Adds Google Client Secret (as Key Vault Reference) to App Configuration.')
resource appConfigGoogleClientSecret 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyGoogleClientSecret // Key matches C# expectation
  properties: {
    value: '{"uri":"${keyVault.properties.vaultUri}secrets/${googleClientSecretSecret.name}"}'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    label: aspNetCoreEnvironmentName // Uses the environmentName parameter directly now
  }
  dependsOn: [ keyVault, googleClientSecretSecret ]
}

@description('Adds JWT Secret as a Key Vault reference to App Configuration.')
resource appConfigJwtSecretRef 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyJwtSecret
  properties: {
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    value: string({
      uri: '${keyVault.properties.vaultUri}secrets/${secretNameJwtSecret}'
    })
    label: aspNetCoreEnvironmentName // Uses the environmentName parameter directly now
  }
  dependsOn: [ jwtSecretSecret, keyVault ]
}

// Add connection string reference to App Configuration
@description('Adds PostgreSQL Connection String to App Configuration as Key Vault reference.')
resource appConfigPostgresConnectionString 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfigStore
  name: configKeyPostgresConnectionString // Uses the wikiquizapp:ConnectionString format
  properties: {
    value: '{"uri":"${keyVault.properties.vaultUri}secrets/${postgresConnectionStringSecret.name}"}'
    contentType: 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
    label: aspNetCoreEnvironmentName
  }
  dependsOn: [keyVault, postgresConnectionStringSecret]
}


// 5. Azure Database for PostgreSQL - Flexible Server (Burstable Tier)
@description('Creates the Azure Database for PostgreSQL Flexible Server.')
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '15'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword // Still needed for initial server creation
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// 5.1. Create Initial Database on Postgres Server
@description('Creates the initial database within the PostgreSQL server.')
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-06-01-preview' = {
  parent: postgresServer
  name: postgresDatabaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// 5.2 Configure Firewall Rule to Allow Azure Services
@description('Configures PostgreSQL firewall to allow access from Azure services.')
resource postgresFirewallRuleAllowAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-06-01-preview' = {
  parent: postgresServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
  dependsOn: [
    postgresServer
  ]
}


// 6. Container Apps Environment (Consumption Plan)
@description('Creates the Azure Container Apps Environment.')
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  sku: {
    name: 'Consumption'
  }
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
  dependsOn: [
    logAnalyticsWorkspace
  ]
}

// --- Role Assignments for Managed Identity ---
@description('Grants the Container App Managed Identity permission to get secrets from Key Vault.')
resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, managedIdentity.id, 'kvSecretUser')
  scope: keyVault
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('Grants the Container App Managed Identity permission to read data from App Configuration.')
resource appConfigDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appConfigStore.id, managedIdentity.id, 'appConfigReader')
  scope: appConfigStore
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071') // App Configuration Data Reader
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('Grants the Container App Managed Identity permission to pull images from ACR.')
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, managedIdentity.id, 'acrPull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// 7. Container App (Web API) - Now using the user-assigned managed identity
@description('Creates the Azure Container App for the Web API.')
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      // --- Secrets Configuration ---
      // secrets: [] // Empty unless needed for other purposes

      // --- Container Registry Configuration ---
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: managedIdentity.id
        }
      ]
      // --- Ingress Configuration ---
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'auto'
        allowInsecure: false // Enforce HTTPS
      }
    }
    // --- Template for the Container(s) ---
    template: {
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
      containers: [
        {
          image: containerImage
          name: 'webapi'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          // --- Environment Variables ---
          env: [
            // App Configuration Endpoint for connecting to App Config
            {
              name: 'AZURE_APP_CONFIG_ENDPOINT'
              value: appConfigStore.properties.endpoint
            }
            // ASP.NET Core Environment Name
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName
            }
            // Managed identity client ID for authentication with Azure services
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentity.properties.clientId
            }
          ]
        }
      ]
    }
  }
  dependsOn: [
    containerAppEnv
    containerRegistry
    keyVault
    appConfigStore
    appConfigPostgresConnectionString
    appConfigOpenAIApiKey
    appConfigGoogleClientId
    appConfigGoogleClientSecret
    appConfigJwtIssuer
    appConfigJwtAudience
    appConfigJwtSecretRef
    postgresFirewallRuleAllowAzure
    acrPullRoleAssignment
    keyVaultSecretUserRoleAssignment
    appConfigDataReaderRoleAssignment
  ]
}


// --- Outputs ---
@description('The fully qualified domain name of the deployed Container App.')
output containerAppUrl string = containerApp.properties.configuration.ingress.fqdn

@description('The login server for the Azure Container Registry.')
output acrLoginServer string = containerRegistry.properties.loginServer

@description('The fully qualified domain name of the PostgreSQL server.')
output postgresFqdn string = postgresServer.properties.fullyQualifiedDomainName

@description('The name of the Azure Key Vault.')
output keyVaultName string = keyVault.name

@description('The URI of the Azure Key Vault.')
output keyVaultUri string = keyVault.properties.vaultUri

@description('The name of the App Configuration store.')
output appConfigStoreName string = appConfigStore.name

@description('The endpoint URI of the App Configuration store.')
output appConfigStoreEndpoint string = appConfigStore.properties.endpoint
