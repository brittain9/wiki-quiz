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
    // Stack Outputs
    [Output] public Output<string> ContainerAppUrl { get; private set; }
    [Output] public Output<string> AcrLoginServer { get; private set; }
    [Output] public Output<string> PostgresFqdn { get; private set; }
    [Output] public Output<string> KeyVaultName { get; private set; }
    [Output] public Output<string> KeyVaultUri { get; private set; }
    [Output] public Output<string> AppConfigStoreName { get; private set; }
    [Output] public Output<string> AppConfigStoreEndpoint { get; private set; }
    [Output] public Output<string> ResourceGroupName { get; private set; }
    [Output] public Output<string> StaticWebAppUrl { get; private set; }
    [Output] public Output<string> StaticWebAppName { get; private set; }
    [Output] public Output<string> StaticWebAppDeploymentToken { get; private set; }

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

        // Resource Group
        var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs
        {
            ResourceGroupName = AzureResourceNaming.GenerateResourceGroupName(config.ProjectName, config.EnvironmentName),
            Location = config.Location,
            Tags = commonTags
        });

        // Generate unique suffix for resources that need it
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

        // Module Instantiation (in dependency order)

        // 1. Managed Identity
        var identityModule = new IdentityModule("identity", new IdentityModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            EnvironmentShort = config.EnvironmentShort,
            Location = config.Location,
            Tags = commonTags
        });

        // 2. Log Analytics Workspace
        var monitoringModule = new MonitoringModule("monitoring", new MonitoringModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            UniqueSuffix = uniqueSuffix,
            EnvironmentShort = config.EnvironmentShort,
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
            EnvironmentShort = config.EnvironmentShort,
            Tags = commonTags
        });

        // 4. Azure Database for PostgreSQL
        var databaseModule = new DatabaseModule("database", new DatabaseModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = config.EnvironmentShort,
            UniqueSuffix = uniqueSuffix,
            Tags = commonTags
        });

        // 5. Azure Key Vault and Secrets
        var secretsManagementModule = new SecretsManagementModule("secretsManagement", new SecretsManagementModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = config.EnvironmentShort,
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
            EnvironmentShort = config.EnvironmentShort,
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


        // 9. Container Apps Environment and App
        var containerAppsModule = new ContainerAppsModule("containerApps", new ContainerAppsModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = config.EnvironmentShort,
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
            DependsOn = new[]
            {
                authorizationModule.KeyVaultSecretsUserRoleAssignment,
                authorizationModule.AppConfigDataReaderRoleAssignment,
                authorizationModule.AcrPullRoleAssignment
            }
        });

        // 10. Static Web App
        var staticWebAppModule = new StaticWebAppModule("staticWebApp", new StaticWebAppModuleArgs
        {
            Config = config,
            ResourceGroup = resourceGroup,
            Location = config.Location,
            EnvironmentShort = config.EnvironmentShort,
            UniqueSuffix = uniqueSuffix,
            BackendUrl = containerAppsModule.ContainerAppUrl,
            Tags = commonTags
        });

        // Assign outputs
        ContainerAppUrl = containerAppsModule.ContainerAppUrl;
        AcrLoginServer = registryModule.AcrLoginServer;
        PostgresFqdn = databaseModule.PostgresServerFqdn;
        KeyVaultName = secretsManagementModule.KeyVault.Name;
        KeyVaultUri = secretsManagementModule.KeyVaultUri;
        AppConfigStoreName = appConfigModule.AppConfigurationStore.Name;
        AppConfigStoreEndpoint = appConfigModule.AppConfigStoreEndpoint;
        ResourceGroupName = resourceGroup.Name;
        StaticWebAppUrl = staticWebAppModule.StaticWebAppUrl;
        StaticWebAppName = staticWebAppModule.StaticWebAppName;
        StaticWebAppDeploymentToken = staticWebAppModule.StaticWebAppDeploymentToken;
        

        this.RegisterOutputs();
    }
}
