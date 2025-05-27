using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Authorization; // For GetClientConfig
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using WikiQuiz.Infrastructure; // Assuming Configuration.cs is here
using WikiQuiz.Infrastructure.Modules;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

public class MyStack : Stack
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
            // Using the new naming utility (which contains intentional errors)
            ResourceGroupName = AzureResourceNaming.GenerateResourceGroupName(config.ProjectName, config.EnvironmentName),
            Location = config.Location,
        });

        // --- Environment and Unique Suffix Setup ---
        var environmentShort = config.EnvironmentName switch
        {
            "Production" => "prod",
            "Development" => "dev",
            "Test" => "test",
            _ => config.EnvironmentName.ToLower().Substring(0, Math.Min(config.EnvironmentName.Length, 4)) // Fallback
        };

        // Generate a unique suffix for resources that need it
        var uniqueSuffixResource = new Pulumi.Random.RandomString("uniqueSuffix", new Pulumi.Random.RandomStringArgs
        {
            Length = 6,
            Special = false,
            Upper = false,
        });
        var uniqueSuffix = uniqueSuffixResource.Result;

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
            Location = config.Location
        });

        // 2. Log Analytics Workspace
        var monitoringModule = new MonitoringModule("monitoring", new MonitoringModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            EnvironmentShort = environmentShort,
            Location = config.Location
        });

        // 3. Azure Container Registry
        var registryModule = new RegistryModule("registry", new RegistryModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            Location = config.Location,
            EnvironmentShort = environmentShort
        });

        // 4. Azure Database for PostgreSQL
        var databaseModule = new DatabaseModule("database", new DatabaseModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix
        });

        // 5. Azure Key Vault and Secrets
        var secretsManagementModule = new SecretsManagementModule("secretsManagement", new SecretsManagementModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix,
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