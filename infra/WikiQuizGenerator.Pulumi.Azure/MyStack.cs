using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.ContainerInstance;
using Pulumi.AzureNative.Web;

public class MyStack : Stack
{
    [Output] public Output<string> ApiUrl { get; private set; }
    [Output] public Output<string> FrontendUrl { get; private set; }
    [Output] public Output<string> DatabaseHost { get; private set; }
    
    // Frontend environment variables as outputs
    [Output] public Output<string> ViteApiBaseUrl { get; private set; }
    [Output] public Output<string> ViteAppBaseUrl { get; private set; }

    public MyStack()
    {
        var config = new Config();
        
        // Get configuration
        var environment = config.Get("environment") ?? "dev";
        var location = config.Get("location") ?? "East US";
        var githubUsername = config.Require("githubUsername"); // Your GitHub username
        
        // Custom domain configuration
        var customApiDomain = config.Get("customApiDomain"); // e.g., "devapi.yourdomain.com" or "api.yourdomain.com"
        var customAppDomain = config.Get("customAppDomain"); // e.g., "devquiz.yourdomain.com" or "quiz.yourdomain.com"
        
        // Get required secrets
        var postgresPassword = config.RequireSecret("postgresPassword");
        var openAiApiKey = config.RequireSecret("openAiApiKey");
        var googleClientId = config.RequireSecret("googleClientId");
        var googleClientSecret = config.RequireSecret("googleClientSecret");
        var jwtSecret = config.RequireSecret("jwtSecret");
        
        // Container image from GitHub Container Registry
        var containerImage = $"ghcr.io/{githubUsername}/wikiquiz-api:{environment}";
        
        // Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            ResourceGroupName = $"rg-wikiquiz-{environment}",
            Location = location,
        });

        // PostgreSQL Flexible Server - Cheapest possible configuration
        var postgresServer = new FlexibleServer("postgres", new FlexibleServerArgs
        {
            ServerName = $"wikiquiz-db-{environment}-{System.Guid.NewGuid().ToString("N")[..6]}",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Version = "13",
            AdministratorLogin = "wikiquizadmin",
            AdministratorLoginPassword = postgresPassword,
            Sku = new SkuArgs
            {
                Name = "Standard_B1ms", // Cheapest: 1 vCore, 2GB RAM
                Tier = "Burstable"
            },
            Storage = new StorageArgs
            {
                StorageSizeGB = 32 // Minimum
            },
            Backup = new BackupArgs
            {
                BackupRetentionDays = 7, // Minimum
                GeoRedundantBackup = "Disabled"
            },
            HighAvailability = new HighAvailabilityArgs
            {
                Mode = "Disabled" // No HA = cheaper
            }
        });

        // Database
        var database = new Database("wikiquizdb", new DatabaseArgs
        {
            DatabaseName = "wikiquiz",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
        });

        // Firewall rule to allow Azure services
        var firewallRule = new FirewallRule("allowAzure", new FirewallRuleArgs
        {
            FirewallRuleName = "AllowAllWindowsAzureIps",
            ResourceGroupName = resourceGroup.Name,
            ServerName = postgresServer.Name,
            StartIpAddress = "0.0.0.0",
            EndIpAddress = "0.0.0.0"
        });

        // Connection string
        var connectionString = Output.Tuple(postgresServer.FullyQualifiedDomainName, postgresPassword)
            .Apply(values => $"Host={values.Item1};Database=wikiquiz;Username=wikiquizadmin;Password={values.Item2};SSL Mode=Require;");

        // Static Web App for Frontend - Free tier
        var staticWebApp = new StaticSite("frontend", new StaticSiteArgs
        {
            Name = $"wikiquiz-frontend-{environment}",
            ResourceGroupName = resourceGroup.Name,
            Location = "Central US", // Static Web Apps limited regions
            Sku = new SkuDescriptionArgs
            {
                Name = "Free",
                Tier = "Free"
            },
            BuildProperties = new StaticSiteBuildPropertiesArgs
            {
                AppLocation = "/frontend",
                OutputLocation = "dist",
            },
        });

        // Container Group - Cheapest container option
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
            Containers = new[]
            {
                new ContainerArgs
                {
                    Name = "wikiquiz-api",
                    Image = containerImage, // ghcr.io/username/wikiquiz-api:dev or :prd
                    Resources = new ResourceRequirementsArgs
                    {
                        Requests = new ResourceRequestsArgs
                        {
                            Cpu = 0.5, // Minimum
                            MemoryInGB = 1.0 // Minimum
                        }
                    },
                    Ports = new[]
                    {
                        new ContainerPortArgs { Port = 80, Protocol = "TCP" }
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
                        new EnvironmentVariableArgs { Name = "WikiQuizApp__FrontendUri", Value = !string.IsNullOrEmpty(customAppDomain) ? $"https://{customAppDomain}" : staticWebApp.DefaultHostname.Apply(h => $"https://{h}") },
                        new EnvironmentVariableArgs { Name = "FORWARDEDHEADERS_ENABLED", Value = "true" }
                    }
                }
            }
        }, new CustomResourceOptions
        {
            DependsOn = new[] { database, firewallRule }
        });

        // Outputs
        ApiUrl = containerGroup.IpAddress.Apply(ip => $"http://{ip.Ip}");
        FrontendUrl = staticWebApp.DefaultHostname.Apply(h => $"https://{h}");
        DatabaseHost = postgresServer.FullyQualifiedDomainName;
        
        // Frontend environment variables as outputs (using custom domains if provided)
        ViteApiBaseUrl = !string.IsNullOrEmpty(customApiDomain) 
            ? Output.Create($"https://{customApiDomain}")
            : containerGroup.IpAddress.Apply(ip => $"http://{ip.Ip}");
        ViteAppBaseUrl = !string.IsNullOrEmpty(customAppDomain)
            ? Output.Create($"https://{customAppDomain}")
            : staticWebApp.DefaultHostname.Apply(h => $"https://{h}");
    }
}
