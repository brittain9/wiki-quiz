using Pulumi;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.DBforPostgreSQL.Inputs;
using BackupArgs = Pulumi.AzureNative.DBforPostgreSQL.Inputs.BackupArgs;
using Pulumi.AzureNative.Resources;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class DatabaseModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class DatabaseModule : ComponentResource
    {
        [Output] public Server PostgresServer { get; private set; }
        [Output] public Database PostgresDatabase { get; private set; }
        [Output] public FirewallRule PostgresFirewallRuleAllowAzure { get; private set; }
        [Output] public Output<string> PostgresServerFqdn { get; private set; }
        [Output] public Output<string> PostgresDatabaseNameOutput { get; private set; }

        public DatabaseModule(string name, DatabaseModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:DatabaseModule", name, options)
        {
            var postgresServerName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GeneratePostgresServerName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            // Use environment-specific configuration
            var dbConfig = args.Config.EnvConfig.Database;
            var skuTier = dbConfig.SkuTier switch
            {
                "GeneralPurpose" => SkuTier.GeneralPurpose,
                "Burstable" => SkuTier.Burstable,
                "MemoryOptimized" => SkuTier.MemoryOptimized,
                _ => SkuTier.Burstable
            };

            var geoRedundantBackup = dbConfig.GeoRedundantBackup 
                ? GeoRedundantBackupEnum.Enabled 
                : GeoRedundantBackupEnum.Disabled;

            PostgresServer = new Server("postgresServer", new ServerArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = postgresServerName,
                Location = args.Location,
                Sku = new SkuArgs
                {
                    Name = dbConfig.SkuName,
                    Tier = skuTier
                },
                AdministratorLogin = args.Config.PostgresAdminLogin,
                AdministratorLoginPassword = args.Config.PostgresAdminPassword,
                Version = "15",
                Storage = new StorageArgs { StorageSizeGB = dbConfig.StorageSizeGB },
                Backup = new BackupArgs 
                { 
                    BackupRetentionDays = dbConfig.BackupRetentionDays, 
                    GeoRedundantBackup = geoRedundantBackup 
                },
                HighAvailability = new HighAvailabilityArgs 
                { 
                    Mode = args.Config.EnvConfig.HighAvailabilityEnabled 
                        ? HighAvailabilityMode.ZoneRedundant 
                        : HighAvailabilityMode.Disabled 
                },
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            // Create database
            PostgresDatabase = new Database("postgresDatabase", new DatabaseArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = PostgresServer.Name,
                DatabaseName = args.Config.PostgresDatabaseName,
                Charset = "UTF8",
                Collation = "en_US.utf8"
            }, new CustomResourceOptions { Parent = this });

            // Allow Azure services to access the database
            PostgresFirewallRuleAllowAzure = new FirewallRule("postgresFirewallRuleAllowAzure", new FirewallRuleArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = PostgresServer.Name,
                FirewallRuleName = "AllowAllWindowsAzureIps",
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            }, new CustomResourceOptions { Parent = this });

            // Set outputs
            PostgresServerFqdn = PostgresServer.FullyQualifiedDomainName;
            PostgresDatabaseNameOutput = Output.Create(args.Config.PostgresDatabaseName);

            this.RegisterOutputs();
        }
    }
}
