using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
// We now EXCLUSIVELY use the V20221201 API version for all PostgreSQL resources.
using Pulumi.AzureNative.DBforPostgreSQL.V20221201;
using Pulumi.AzureNative.DBforPostgreSQL.V20221201.Inputs;
using Pulumi.AzureNative.OperationalInsights;

public class MyStack : Stack
{
    [Output] public Output<string> ApiUrl { get; private set; }
    [Output] public Output<string> FrontendUrl { get; private set; }
    [Output] public Output<string> DatabaseHost { get; private set; }
    [Output] public Output<string> DatabaseConnectionString { get; private set; }
    [Output] public Output<string> LogAnalyticsWorkspaceId { get; private set; }

    public MyStack()
    {
        var config = new Config("wikiquiz");
        
        var environment = config.Get("environment");
        var location = config.Get("location");
        
        var frontendUri = config.Get("frontendUri");
        var jwtIssuer = config.Get("jwtIssuer");
        var jwtAudience = config.Get("jwtAudience");
        
        var postgresPassword = config.RequireSecret("postgresPassword");
        var openAiApiKey = config.RequireSecret("openAiApiKey");
        var googleClientId = config.RequireSecret("googleClientId");
        var googleClientSecret = config.RequireSecret("googleClientSecret");
        var jwtSecret = config.RequireSecret("jwtSecret");

        var ghcrUsername = config.Require("ghcrUsername");
        var ghcrPassword = config.RequireSecret("ghcrPassword"); 

        var containerImage = $"ghcr.io/brittain9/wiki-quiz:{environment}";
        
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            ResourceGroupName = $"rg-wikiquiz-{environment}",
            Location = location,
        });

        var postgresServer = new Server("postgres", new ServerArgs
        {
            ServerName = $"wikiquiz-db-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new SkuArgs
            {
                Name = "Standard_B1ms",
                Tier = SkuTier.Burstable
            },
            Storage = new StorageArgs
            {
                StorageSizeGB = 32
            },
            Backup = new BackupArgs
            {
                BackupRetentionDays = 7,
                GeoRedundantBackup = "Disabled"
            },
            HighAvailability = new HighAvailabilityArgs
            {
                Mode = HighAvailabilityMode.Disabled
            },
            AdministratorLogin = "wikiquizadmin",
            AdministratorLoginPassword = postgresPassword,
            Version = ServerVersion.ServerVersion_14,
            Tags =
            {
                { "Environment", environment },
                { "Project", "WikiQuiz" },
                { "CostCenter", "Development" }
            }
        });

        var firewallRuleAzure = new FirewallRule("allow-azure", new FirewallRuleArgs
        {
            FirewallRuleName = "AllowAzureServices",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
            StartIpAddress = "0.0.0.0",
            EndIpAddress = "0.0.0.0"
        });

        var firewallRuleAll = new FirewallRule("allow-all-dev", new FirewallRuleArgs
        {
            FirewallRuleName = "AllowAllDev",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
            StartIpAddress = "0.0.0.0",
            EndIpAddress = "255.255.255.255"
        });

        var database = new Database("database", new DatabaseArgs
        {
            DatabaseName = "wikiquiz",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
            Charset = "UTF8",
            Collation = "en_US.utf8"
        });

        var connectionString = Output.Tuple(postgresServer.FullyQualifiedDomainName, postgresPassword)
            .Apply(t => $"Server={t.Item1};Database=wikiquiz;Username=wikiquizadmin;Password={t.Item2};SSL Mode=Require;");

        // Create Log Analytics Workspace for Container Apps logging (optimized for cost)
        var logAnalyticsWorkspace = new Workspace("log-analytics", new WorkspaceArgs
        {
            WorkspaceName = $"wikiquiz-logs-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new Pulumi.AzureNative.OperationalInsights.Inputs.WorkspaceSkuArgs
            {
                Name = "PerGB2018" // Most cost-effective for low volume
            },
            RetentionInDays = 30, // Minimum retention for PerGB2018 SKU
            Tags = 
            {
                { "Environment", environment },
                { "Project", "WikiQuiz" },
                { "CostCenter", "Development" }
            }
        });

        // Get the shared keys for the Log Analytics workspace
        var workspaceKeys = Pulumi.AzureNative.OperationalInsights.GetSharedKeys.Invoke(new Pulumi.AzureNative.OperationalInsights.GetSharedKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = logAnalyticsWorkspace.Name
        });

        // Create Container Apps Environment
        var containerEnvironment = new ManagedEnvironment("env", new ManagedEnvironmentArgs
        {
            EnvironmentName = $"wikiquiz-env-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = logAnalyticsWorkspace.CustomerId,
                    SharedKey = workspaceKeys.Apply(keys => keys.PrimarySharedKey)
                }
            },
            Tags = 
            {
                { "Environment", environment },
                { "Project", "WikiQuiz" },
                { "CostCenter", "Development" }
            }
        });

        // Create Container App
        var containerApp = new ContainerApp("api", new ContainerAppArgs
        {
            ContainerAppName = $"wikiquiz-api-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            ManagedEnvironmentId = containerEnvironment.Id,
            Configuration = new Pulumi.AzureNative.App.Inputs.ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 80,
                    Transport = IngressTransportMethod.Http,
                    CorsPolicy = new CorsPolicyArgs
                    {
                        AllowCredentials = true,
                        AllowedOrigins = new[] { frontendUri },
                        AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
                        AllowedHeaders = new[] { "*" },
                        MaxAge = 3600
                    },
                    Traffic = new[]
                    {
                        new TrafficWeightArgs
                        {
                            Weight = 100,
                            LatestRevision = true
                        }
                    }
                },
                Registries = new[]
                {
                    new RegistryCredentialsArgs
                    {
                        Server = "ghcr.io",
                        Username = ghcrUsername,
                        PasswordSecretRef = "ghcr-password"
                    }
                },
                Secrets = new[]
                {
                    new SecretArgs { Name = "ghcr-password", Value = ghcrPassword },
                    new SecretArgs { Name = "connection-string", Value = connectionString },
                    new SecretArgs { Name = "openai-key", Value = openAiApiKey },
                    new SecretArgs { Name = "google-client-id", Value = googleClientId },
                    new SecretArgs { Name = "google-client-secret", Value = googleClientSecret },
                    new SecretArgs { Name = "jwt-secret", Value = jwtSecret }
                }
            },
            Template = new TemplateArgs
            {
                Scale = new ScaleArgs
                {
                    MinReplicas = 0,
                    MaxReplicas = 10,
                    Rules = new[]
                    {
                        new ScaleRuleArgs
                        {
                            Name = "http-rule",
                            Http = new HttpScaleRuleArgs
                            {
                                Metadata = 
                                {
                                    { "concurrentRequests", "50" }
                                }
                            }
                        }
                    }
                },
                Containers = new[]
                {
                    new ContainerArgs
                    {
                        Name = "wiki-quiz",
                        Image = containerImage,
                        Resources = new ContainerResourcesArgs
                        {
                            Cpu = 0.5,
                            Memory = "1Gi"
                        },
                        Env = new[]
                        {
                            new EnvironmentVarArgs { Name = "ConnectionStrings__DefaultConnection", SecretRef = "connection-string" },
                            new EnvironmentVarArgs { Name = "OpenAI__ApiKey", SecretRef = "openai-key" },
                            new EnvironmentVarArgs { Name = "GoogleAuth__ClientId", SecretRef = "google-client-id" },
                            new EnvironmentVarArgs { Name = "GoogleAuth__ClientSecret", SecretRef = "google-client-secret" },
                            new EnvironmentVarArgs { Name = "JwtOptions__Secret", SecretRef = "jwt-secret" },
                            new EnvironmentVarArgs { Name = "JwtOptions__Issuer", Value = jwtIssuer },
                            new EnvironmentVarArgs { Name = "JwtOptions__Audience", Value = jwtAudience },
                            new EnvironmentVarArgs { Name = "WikiQuizApp__FrontendUri", Value = frontendUri },
                            new EnvironmentVarArgs { Name = "ASPNETCORE_ENVIRONMENT", Value = environment == "dev" ? "Development" : "Production" },
                            new EnvironmentVarArgs { Name = "ASPNETCORE_HTTP_PORTS", Value = "80" }
                        }
                    }
                }
            },
            Tags = 
            {
                { "Environment", environment },
                { "Project", "WikiQuiz" },
                { "CostCenter", "Development" }
            }
        });

        ApiUrl = containerApp.Configuration.Apply(config => $"https://{config!.Ingress!.Fqdn}");
        FrontendUrl = Output.Create("https://frontend-placeholder.com"); // Placeholder until Static Web App is configured
        DatabaseHost = postgresServer.FullyQualifiedDomainName;
        DatabaseConnectionString = connectionString;
        LogAnalyticsWorkspaceId = logAnalyticsWorkspace.Id;
    }
}