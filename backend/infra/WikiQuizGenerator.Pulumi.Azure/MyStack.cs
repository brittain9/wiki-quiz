using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Authorization; // For GetClientConfig
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using WikiQuiz.Infrastructure; // Assuming Configuration.cs is here
using WikiQuiz.Infrastructure.Modules;

class MyStack : Stack
{
    // --- Stack Outputs ---
    [Output] public Output<string> ContainerAppUrl { get; private set; }
    [Output] public Output<string> AcrLoginServer { get; private set; }
    [Output] public Output<string> PostgresFqdn { get; private set; }
    [Output] public Output<string> KeyVaultName { get; private set; }
    [Output] public Output<string> KeyVaultUri { get; private set; }
    [Output] public Output<string> AppConfigStoreName { get; private set; }
    [Output] public Output<string> AppConfigStoreEndpoint { get; private set; }

    public MyStack()
    {
        var config = new StackConfig();

        var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs
        {
            // Resource group name from Bicep: rg-wikiquizapi-development (example)
            ResourceGroupName = $"rg-{config.ProjectName.ToLower()}-{config.EnvironmentName.ToLower()}",
            Location = config.Location,
        });

        // --- Naming Convention Variables ( mirroring Bicep logic ) ---
        var environmentShort = config.EnvironmentName switch
        {
            "Production" => "prod",
            "Development" => "dev",
            "Test" => "test",
            _ => config.EnvironmentName.ToLower().Substring(0, Math.Min(config.EnvironmentName.Length, 4)) // Fallback
        };

        // Bicep's uniqueString is based on resourceGroup.id, projectName, location.
        // We will generate a similar unique suffix. Pulumi's Random provider is good for this.
        // For now, a placeholder to be replaced by a RandomString resource.
        var uniqueSuffixResource = new Pulumi.Random.RandomString("uniqueSuffix", new Pulumi.Random.RandomStringArgs
        {
            Length = 6,
            Special = false,
            Upper = false,
        });
        var uniqueSuffix = uniqueSuffixResource.Result;

        // Helper function for safe substring and lowercasing, similar to Bicep's pattern
        Func<string, int, string> SanitizeName = (name, maxLength) => 
        {
            var lower = name.ToLower();
            return lower.Length <= maxLength ? lower : lower.Substring(0, maxLength);
        };

        // Helper to remove non-alphanumeric for specific resources like ACR
        Func<string, int, string> SanitizeAlphaNumeric = (name, maxLength) =>
        {
            var replaced = Regex.Replace(name, "[^a-zA-Z0-9]", "");
            return SanitizeName(replaced, maxLength);
        };

        // Get current Azure tenant ID for Key Vault
        var currentClient = GetClientConfig.InvokeAsync().Result;
        var tenantId = Output.Create(currentClient.TenantId);

        // --- Module Instantiation (in dependency order) ---

        // 1. Managed Identity
        var identityModule = new IdentityModule("identity", new IdentityModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            EnvironmentShort = environmentShort,
            SanitizeName = SanitizeName,
            Location = config.Location
        });

        // 2. Log Analytics Workspace
        var monitoringModule = new MonitoringModule("monitoring", new MonitoringModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            SanitizeName = SanitizeName,
            Location = config.Location
        });

        // 3. Azure Container Registry
        var registryModule = new RegistryModule("registry", new RegistryModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            Location = config.Location,
            SanitizeAlphaNumeric = SanitizeAlphaNumeric
        });

        // 4. Azure Database for PostgreSQL
        var databaseModule = new DatabaseModule("database", new DatabaseModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix,
            SanitizeName = SanitizeName
        });

        // 5. Azure Key Vault and Secrets
        var secretsManagementModule = new SecretsManagementModule("secretsManagement", new SecretsManagementModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix,
            SanitizeName = SanitizeName,
            TenantId = tenantId,
            PostgresServerFqdn = databaseModule.PostgresServerFqdn,
            PostgresDatabaseNameOutput = databaseModule.PostgresDatabaseNameOutput
        });

        // 6. Azure App Configuration
        var appConfigModule = new AppConfigModule("appConfig", new AppConfigModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix,
            SanitizeName = SanitizeName,
            KeyVaultUri = secretsManagementModule.KeyVaultUri,
            OpenAIApiKeySecretName = secretsManagementModule.SecretNameOpenAIApiKey,
            GoogleClientIdSecretName = secretsManagementModule.SecretNameGoogleClientId,
            GoogleClientSecretSecretName = secretsManagementModule.SecretNameGoogleClientSecret,
            JwtSecretKvName = secretsManagementModule.SecretNameJwtSecret,
            PostgresConnectionStringSecretName = secretsManagementModule.SecretNamePostgresConnectionString,
            KeyVaultResource = secretsManagementModule.KeyVault
        });

        // 7. Role Assignments
        var authorizationModule = new AuthorizationModule("authorization", new AuthorizationModuleArgs
        {
            UserAssignedIdentity = identityModule.UserAssignedIdentity,
            KeyVault = secretsManagementModule.KeyVault,
            AppConfigStore = appConfigModule.AppConfigurationStore,
            AcrRegistry = registryModule.AcrRegistry
        });

        // 8. Container Apps Environment and Container App
        var containerAppsModule = new ContainerAppsModule("containerApps", new ContainerAppsModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            SanitizeName = SanitizeName,
            LogAnalyticsWorkspaceId = monitoringModule.LogAnalyticsWorkspaceId,
            LogAnalyticsWorkspaceSharedKey = monitoringModule.LogAnalyticsWorkspaceSharedKey,
            UserAssignedIdentity = identityModule.UserAssignedIdentity,
            AcrLoginServer = registryModule.AcrLoginServer,
            AppConfigStoreEndpoint = appConfigModule.AppConfigStoreEndpoint,
            KeyVaultUri = secretsManagementModule.KeyVaultUri,
            AppConfigStore = appConfigModule.AppConfigurationStore
        }, new ComponentResourceOptions
        {
            DependsOn = 
            {
                // Ensure role assignments are completed before Container App creation
                authorizationModule.KeyVaultSecretsUserRoleAssignment,
                authorizationModule.AppConfigDataReaderRoleAssignment,
                authorizationModule.AcrPullRoleAssignment
            }
        });

        // --- Assign outputs ---
        ContainerAppUrl = containerAppsModule.ContainerAppFqdn;
        AcrLoginServer = registryModule.AcrLoginServer;
        PostgresFqdn = databaseModule.PostgresServerFqdn;
        KeyVaultName = secretsManagementModule.KeyVault.Name;
        KeyVaultUri = secretsManagementModule.KeyVaultUri;
        AppConfigStoreName = appConfigModule.AppConfigurationStore.Name;
        AppConfigStoreEndpoint = appConfigModule.AppConfigStoreEndpoint;
    }
} 