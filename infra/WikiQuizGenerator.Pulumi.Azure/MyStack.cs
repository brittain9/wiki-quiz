using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.DocumentDB.Inputs;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Insights.Inputs;

public class MyStack : Stack
{
    [Output] public Output<string> ApiUrl { get; private set; }
    [Output] public Output<string> FrontendUrl { get; private set; }
    [Output] public Output<string> ApplicationInsightsConnectionString { get; private set; }
    [Output] public Output<string> LogAnalyticsWorkspaceId { get; private set; }

    public MyStack()
    {
        var config = new Config("wikiquiz");

        var location = config.Require("location");

        // Container resources (small footprint; scale-to-zero handles idle time)
        var containerCpu = 0.25;
        var containerMemory = "0.5Gi";
        var logAnalyticsRetention = 30;
        
        var frontendUri = config.Require("frontendUri");
        var jwtIssuer = config.Get("jwtIssuer"); // Can be null if not set
        var jwtAudience = config.Get("jwtAudience"); // Can be null if not set
        
        var openAiApiKey = config.RequireSecret("openAiApiKey");
        var googleClientId = config.RequireSecret("googleClientId");
        var googleClientSecret = config.RequireSecret("googleClientSecret");
        var jwtSecret = config.RequireSecret("jwtSecret");

        var ghcrUsername = config.Require("ghcrUsername");
        var ghcrPassword = config.RequireSecret("ghcrPassword"); 

        // Allow overriding the image tag via config for CI/CD (e.g., commit SHA). Defaults to "prd".
        var imageTag = config.Get("imageTag") ?? "prd";
        var containerImage = Output.Format($"ghcr.io/{ghcrUsername}/wiki-quiz:{imageTag}");
        
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            ResourceGroupName = $"rg-wikiquiz-prd",
            Location = location,
        });

        // Create Log Analytics Workspace for Container Apps logging
        var logAnalyticsWorkspace = new Workspace("log-analytics", new WorkspaceArgs
        {
            WorkspaceName = $"wikiquiz-logs-prd",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new Pulumi.AzureNative.OperationalInsights.Inputs.WorkspaceSkuArgs
            {
                Name = "PerGB2018" // Most cost-effective for low volume
            },
            RetentionInDays = logAnalyticsRetention, // Minimum retention for PerGB2018 SKU
            Tags = { { "Project", "WikiQuiz" }, { "Environment", "prd" } }
        });

        // Get the shared keys for the Log Analytics workspace
        var workspaceKeys = Pulumi.AzureNative.OperationalInsights.GetSharedKeys.Invoke(new Pulumi.AzureNative.OperationalInsights.GetSharedKeysInvokeArgs
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = logAnalyticsWorkspace.Name
        });

        // Create Container Apps Environment (required; logs routed to Log Analytics)
        var containerEnvironment = new ManagedEnvironment("env", new ManagedEnvironmentArgs
        {
            EnvironmentName = $"wikiquiz-env-prd",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            AppLogsConfiguration = new AppLogsConfigurationArgs
            {
                Destination = "log-analytics",
                LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                {
                    CustomerId = logAnalyticsWorkspace.CustomerId,
                    // Use null-forgiving operator as PrimarySharedKey is expected to be non-null
                    SharedKey = workspaceKeys.Apply(keys => keys.PrimarySharedKey!)
                }
            }
        });

        // Cosmos DB account (Free Tier) and SQL database
        var cosmosAccount = new DatabaseAccount("cosmos", new DatabaseAccountArgs
        {
            AccountName = $"cosmos-wikiquiz-prd",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Kind = DatabaseAccountKind.GlobalDocumentDB,
            DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
            Capabilities =
            {
                new CapabilityArgs { Name = "EnableServerless" }
            },
            ConsistencyPolicy = new ConsistencyPolicyArgs
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
            },
            Locations =
            {
                new LocationArgs
                {
                    FailoverPriority = 0,
                    LocationName = resourceGroup.Location,
                }
            },
            Tags = { { "Project", "WikiQuiz" }, { "Environment", "prd" } }
        });

        var cosmosDatabase = new SqlResourceSqlDatabase("cosmos-db", new SqlResourceSqlDatabaseArgs
        {
            AccountName = cosmosAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            DatabaseName = "WikiQuizDb",
            Resource = new SqlDatabaseResourceArgs
            {
                Id = "WikiQuizDb"
            }
        });

        // Create containers expected by the app with correct partition keys
        var quizzesContainer = new SqlResourceSqlContainer("cosmos-quizzes", new SqlResourceSqlContainerArgs
        {
            AccountName = cosmosAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            DatabaseName = cosmosDatabase.Name,
            ContainerName = "Quizzes",
            Resource = new SqlContainerResourceArgs
            {
                Id = "Quizzes",
                PartitionKey = new ContainerPartitionKeyArgs
                {
                    Paths = { "/partitionKey" },
                    Kind = "Hash"
                }
            }
        });

        var usersContainer = new SqlResourceSqlContainer("cosmos-users", new SqlResourceSqlContainerArgs
        {
            AccountName = cosmosAccount.Name,
            ResourceGroupName = resourceGroup.Name,
            DatabaseName = cosmosDatabase.Name,
            ContainerName = "Users",
            Resource = new SqlContainerResourceArgs
            {
                Id = "Users",
                PartitionKey = new ContainerPartitionKeyArgs
                {
                    Paths = { "/partitionKey" },
                    Kind = "Hash"
                },
                UniqueKeyPolicy = new UniqueKeyPolicyArgs
                {
                    UniqueKeys =
                    {
                        new UniqueKeyArgs { Paths = { "/email" } }
                    }
                }
            }
        });

        // Get Cosmos connection string
        var cosmosConnectionStrings = ListDatabaseAccountConnectionStrings.Invoke(new ListDatabaseAccountConnectionStringsInvokeArgs
        {
            AccountName = cosmosAccount.Name,
            ResourceGroupName = resourceGroup.Name
        });

        var cosmosConnectionString = cosmosConnectionStrings.Apply(r => r.ConnectionStrings[0].ConnectionString);

        // Application Insights (for telemetry)
        var appInsights = new Component("appinsights", new ComponentArgs
        {
            ResourceName = $"wikiquiz-ai-prd",
            ResourceGroupName = resourceGroup.Name,
            ApplicationType = ApplicationType.Web,
            Kind = "web",
            IngestionMode = IngestionMode.LogAnalytics,
            WorkspaceResourceId = logAnalyticsWorkspace.Id,
            Tags = { { "Project", "WikiQuiz" }, { "Environment", "prd" } }
        });

        var appInsightsConnectionString = appInsights.ConnectionString;

        // Create Container App (scale-to-zero)
        var containerApp = new ContainerApp("api", new ContainerAppArgs
        {
            ContainerAppName = $"wikiquiz-api-prd",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            ManagedEnvironmentId = containerEnvironment.Id,
            Configuration = new Pulumi.AzureNative.App.Inputs.ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = true,
                    TargetPort = 8080,
                    Transport = IngressTransportMethod.Http,
                    CorsPolicy = new Pulumi.AzureNative.App.Inputs.CorsPolicyArgs
                    {
                        AllowCredentials = true,
                        // Ensure frontendUri is non-nullable by using config.Require
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
                    new SecretArgs { Name = "cosmos-connection", Value = cosmosConnectionString },
                    new SecretArgs { Name = "appinsights-connection", Value = appInsightsConnectionString },
                    new SecretArgs { Name = "openai-key", Value = openAiApiKey },
                    new SecretArgs { Name = "google-client-id", Value = googleClientId },
                    new SecretArgs { Name = "google-client-secret", Value = googleClientSecret },
                    new SecretArgs { Name = "jwt-secret", Value = jwtSecret }
                }
            },
            Template = new TemplateArgs
            {
                // Force a new revision name per deploy so ACA rolls even if image caching occurs
                RevisionSuffix = imageTag,
                Scale = new ScaleArgs
                {
                    MinReplicas = 0,
                    MaxReplicas = 10,
                    Rules = new[]
                    {
                        new Pulumi.AzureNative.App.Inputs.ScaleRuleArgs
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
                            Cpu = containerCpu,
                            Memory = containerMemory
                        },
                        Env = new[]
                        {
                            // Cosmos DB configuration
                            new EnvironmentVarArgs { Name = "CosmosDb__ConnectionString", SecretRef = "cosmos-connection" },
                            new EnvironmentVarArgs { Name = "CosmosDb__DatabaseName", Value = "WikiQuizDb" },
                            new EnvironmentVarArgs { Name = "CosmosDb__QuizContainerName", Value = "Quizzes" },
                            new EnvironmentVarArgs { Name = "CosmosDb__UserContainerName", Value = "Users" },
                            // Application Insights connection (the app can enable later)
                            new EnvironmentVarArgs { Name = "APPLICATIONINSIGHTS_CONNECTION_STRING", SecretRef = "appinsights-connection" },
                            // App secrets
                             new EnvironmentVarArgs { Name = "wikiquizapp__FrontendUri", Value = frontendUri },
                            new EnvironmentVarArgs { Name = "wikiquizapp__OpenAIApiKey", SecretRef = "openai-key" },
                            new EnvironmentVarArgs { Name = "wikiquizapp__GoogleOAuthClientId", SecretRef = "google-client-id" },
                            new EnvironmentVarArgs { Name = "wikiquizapp__GoogleOAuthClientSecret", SecretRef = "google-client-secret" },
                            new EnvironmentVarArgs { Name = "wikiquizapp__JwtSecret", SecretRef = "jwt-secret" },
                            new EnvironmentVarArgs { Name = "ASPNETCORE_ENVIRONMENT", Value = "Production" },
                            new EnvironmentVarArgs { Name = "ASPNETCORE_URLS", Value = "http://+:8080" }
                        }
                    }
                }
            },
            Tags = 
            {
                { "Environment", "prd" },
                { "Project", "WikiQuiz" },
                { "CostCenter", "Development" }
            }
        });

        // Use null-forgiving operator as Fqdn is expected to be non-null when ingress is external
        ApiUrl = containerApp.Configuration.Apply(config => $"https://{config!.Ingress!.Fqdn!}"); 
        FrontendUrl = Output.Create(frontendUri);
        ApplicationInsightsConnectionString = appInsightsConnectionString;
        LogAnalyticsWorkspaceId = logAnalyticsWorkspace.Id;
    }
}