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
    [Output] public Output<string> ResourceGroupName { get; private set; }
    [Output] public Output<string>? CustomDomainUrl { get; private set; }

    public MyStack()
    {
        var config = new StackConfig();

        // Common tags for all resources
        var commonTags = new Dictionary<string, string>
        {
            { "Project", config.ProjectName },
            { "Environment", config.EnvironmentName },
            { "ManagedBy", "Pulumi" },
            { "CreatedDate", DateTime.UtcNow.ToString("yyyy-MM-dd") },
            { "Owner", "WikiQuiz Team" }
        };

        var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs
        {
            ResourceGroupName = AzureResourceNaming.GenerateResourceGroupName(config.ProjectName, config.EnvironmentName),
            Location = config.Location,
            Tags = commonTags
        });

        // --- Environment and Unique Suffix Setup ---
        var environmentShort = config.EnvironmentName switch
        {
            "Production" => "prd",
            "Development" => "dev",
            "Test" => "tst",
            _ => config.EnvironmentName.ToLower().Substring(0, Math.Min(config.EnvironmentName.Length, 3))
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
            Location = config.Location,
            Tags = commonTags
        });

        // 2. Log Analytics Workspace
        var monitoringModule = new MonitoringModule("monitoring", new MonitoringModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            EnvironmentShort = environmentShort,
            Location = config.Location,
            Tags = commonTags
        });

        // 3. Azure Container Registry
        var registryModule = new RegistryModule("registry", new RegistryModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            Tags = commonTags
        });

        // 4. Azure Database for PostgreSQL
        var databaseModule = new DatabaseModule("database", new DatabaseModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            UniqueSuffix = uniqueSuffix,
            Tags = commonTags
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
            PostgresDatabaseNameOutput = databaseModule.PostgresDatabaseNameOutput,
            Tags = commonTags
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
            KeyVaultResource = secretsManagementModule.KeyVault,
            Tags = commonTags
        });

        // 7. Role Assignments
        var authorizationModule = new AuthorizationModule("authorization", new AuthorizationModuleArgs
        {
            UserAssignedIdentity = identityModule.UserAssignedIdentity,
            KeyVault = secretsManagementModule.KeyVault,
            AppConfigStore = appConfigModule.AppConfigurationStore,
            AcrRegistry = registryModule.AcrRegistry
        });

        // 8. Container Apps Environment (needed for custom domain)
        var containerAppsEnvModule = new ContainerAppsModule("containerAppsEnv", new ContainerAppsModuleArgs
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
            AppConfigStore = appConfigModule.AppConfigurationStore,
            Tags = commonTags
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

        // 9. Custom Domain (if enabled)
        var customDomainModule = new CustomDomainModule("customDomain", new CustomDomainModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = environmentShort,
            ContainerAppEnvironment = containerAppsEnvModule.ContainerAppEnvironment,
            Tags = commonTags
        });

        // 10. Update Container App with custom domain (if enabled)
        if (config.EnableCustomDomain && !string.IsNullOrEmpty(config.CustomDomain))
        {
            // We need to recreate the container app with custom domain support
            var containerAppsWithDomainModule = new ContainerAppsModule("containerAppsWithDomain", new ContainerAppsModuleArgs
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
                AppConfigStore = appConfigModule.AppConfigurationStore,
                Tags = commonTags,
                ManagedCertificate = customDomainModule.ManagedCertificate
            }, new ComponentResourceOptions
            {
                DependsOn = 
                {
                    authorizationModule.KeyVaultSecretsUserRoleAssignment,
                    authorizationModule.AppConfigDataReaderRoleAssignment,
                    authorizationModule.AcrPullRoleAssignment,
                    customDomainModule
                }
            });

            // Use the custom domain container app for outputs
            ContainerAppUrl = containerAppsWithDomainModule.ContainerAppUrl;
            CustomDomainUrl = customDomainModule.CustomDomainUrl;
        }
        else
        {
            // Use the regular container app for outputs
            ContainerAppUrl = containerAppsEnvModule.ContainerAppUrl;
        }

        // --- Assign outputs ---
        AcrLoginServer = registryModule.AcrLoginServer;
        PostgresFqdn = databaseModule.PostgresServerFqdn;
        KeyVaultName = secretsManagementModule.KeyVault.Name;
        KeyVaultUri = secretsManagementModule.KeyVaultUri;
        AppConfigStoreName = appConfigModule.AppConfigurationStore.Name;
        AppConfigStoreEndpoint = appConfigModule.AppConfigStoreEndpoint;
        ResourceGroupName = resourceGroup.Name;
    }
} 