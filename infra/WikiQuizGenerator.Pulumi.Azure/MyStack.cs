using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ContainerInstance;
using Pulumi.AzureNative.ContainerInstance.Inputs;
using Pulumi.AzureNative.Web;
// Corrected: Removed ambiguous, unversioned using statements.
// We now EXCLUSIVELY use the V20221201 API version for all PostgreSQL resources.
using Pulumi.AzureNative.DBforPostgreSQL.V20221201;
using Pulumi.AzureNative.DBforPostgreSQL.V20221201.Inputs;

// The base class should be 'Stack'
public class MyStack : Stack
{
    [Output] public Output<string> ApiUrl { get; private set; }
    [Output] public Output<string> FrontendUrl { get; private set; }
    [Output] public Output<string> DatabaseHost { get; private set; }
    [Output] public Output<string> DatabaseConnectionString { get; private set; }

    public MyStack()
    {
        var config = new Config();
        
        // Get configuration
        var environment = config.Get("environment") ?? "dev";
        var location = config.Get("location") ?? "Central US";
        
        // Get URL configurations
        var frontendUri = config.Get("frontendUri") ?? "http://localhost:5173";
        var jwtIssuer = config.Get("jwtIssuer") ?? "http://localhost:5543";
        var jwtAudience = config.Get("jwtAudience") ?? "http://localhost:5173";
        
        // Get required secrets
        var postgresPassword = config.RequireSecret("postgresPassword");
        var openAiApiKey = config.RequireSecret("openAiApiKey");
        var googleClientId = config.RequireSecret("googleClientId");
        var googleClientSecret = config.RequireSecret("googleClientSecret");
        var jwtSecret = config.RequireSecret("jwtSecret");

        // New: Get GitHub Container Registry credentials from config
        var ghcrUsername = config.Require("ghcrUsername");
        var ghcrPassword = config.RequireSecret("ghcrPassword"); // This will be your GitHub PAT

        // Container image from GitHub Container Registry
        var containerImage = $"ghcr.io/brittain9/wiki-quiz:{environment}";
        
        // Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            ResourceGroupName = $"rg-wikiquiz-{environment}",
            Location = location,
        });

        // PostgreSQL Flexible Server - Cheapest Burstable tier
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

        // Re-added FirewallRule as a separate resource, using the versioned class.
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

        // Database resource, also using the versioned provider
        var database = new Database("database", new DatabaseArgs
        {
            DatabaseName = "wikiquiz",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
            Charset = "UTF8",
            Collation = "en_US.utf8"
        });

        // Build connection string
        var connectionString = Output.Tuple(postgresServer.FullyQualifiedDomainName, postgresPassword)
            .Apply(t => $"Server={t.Item1};Database=wikiquiz;Username=wikiquizadmin;Password={t.Item2};SSL Mode=Require;");

        // Static Web App for Frontend - Free tier
        var staticWebApp = new StaticSite("frontend", new StaticSiteArgs
        {
            Name = $"wikiquiz-frontend-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = "Central US",
            Sku = new Pulumi.AzureNative.Web.Inputs.SkuDescriptionArgs
            {
                Name = "Free",
                Tier = "Free"
            },
            BuildProperties = new Pulumi.AzureNative.Web.Inputs.StaticSiteBuildPropertiesArgs
            {
                AppLocation = "/frontend",
                OutputLocation = "dist",
            },
            Tags = 
            {
                { "Environment", environment },
                { "Project", "WikiQuiz" }
            }
        });

        // Container Group - Optimized for cost and basic scaling
        var containerGroup = new ContainerGroup("api", new ContainerGroupArgs
        {
            ContainerGroupName = $"wikiquiz-api-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            OsType = "Linux",
            RestartPolicy = "Always",
            IpAddress = new IpAddressArgs
            {
                Type = "Public",
                Ports = new[]
                {
                    new PortArgs { Port = 80, Protocol = "TCP" }
                }
            },
            // New: Provide credentials to pull the image from the private registry
            ImageRegistryCredentials = 
            {
                new ImageRegistryCredentialArgs
                {
                    Server = "ghcr.io",
                    Username = ghcrUsername,
                    Password = ghcrPassword
                }
            },
            Containers = new[]
            {
                new ContainerArgs
                {
                    Name = "wiki-quiz",
                    Image = containerImage,
                    Resources = new ResourceRequirementsArgs
                    {
                        Requests = new ResourceRequestsArgs
                        {
                            Cpu = 1.0, 
                            MemoryInGB = 2.0
                        },
                        Limits = new ResourceLimitsArgs
                        {
                            Cpu = 1.0,
                            MemoryInGB = 2.0
                        }
                    },
                    Ports = new[]
                    {
                        new ContainerPortArgs { Port = 80, Protocol = "TCP" }
                    },
                    LivenessProbe = new ContainerProbeArgs
                    {
                        HttpGet = new ContainerHttpGetArgs
                        {
                            Path = "/health",
                            Port = 80,
                            Scheme = "Http"
                        },
                        InitialDelaySeconds = 30,
                        PeriodSeconds = 10,
                        TimeoutSeconds = 5,
                        FailureThreshold = 3
                    },
                    EnvironmentVariables = new[]
                    {
                        new EnvironmentVariableArgs { Name = "ASPNETCORE_ENVIRONMENT", Value = environment },
                        new EnvironmentVariableArgs { Name = "SKIP_APP_CONFIG", Value = "true" },
                        new EnvironmentVariableArgs { Name = "ConnectionStrings__DefaultConnection", SecureValue = connectionString },
                        new EnvironmentVariableArgs { Name = "OpenAI__ApiKey", SecureValue = openAiApiKey },
                        new EnvironmentVariableArgs { Name = "GoogleAuth__ClientId", SecureValue = googleClientId },
                        new EnvironmentVariableArgs { Name = "GoogleAuth__ClientSecret", SecureValue = googleClientSecret },
                        new EnvironmentVariableArgs { Name = "JwtOptions__Secret", SecureValue = jwtSecret },
                        new EnvironmentVariableArgs { Name = "JwtOptions__Issuer", Value = jwtIssuer },
                        new EnvironmentVariableArgs { Name = "JwtOptions__Audience", Value = jwtAudience },
                        new EnvironmentVariableArgs { Name = "WikiQuizApp__FrontendUri", Value = frontendUri },
                        new EnvironmentVariableArgs { Name = "FORWARDEDHEADERS_ENABLED", Value = "true" }
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

        // Outputs
        ApiUrl = containerGroup.IpAddress.Apply(ip => $"http://{ip!.Ip}");
        FrontendUrl = staticWebApp.DefaultHostname.Apply(h => $"https://{h}");
        DatabaseHost = postgresServer.FullyQualifiedDomainName;
        DatabaseConnectionString = connectionString;
    }
}