using Pulumi;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.DBforPostgreSQL.Inputs;
using Pulumi.AzureNative.Resources;
using System;

namespace WikiQuiz.Infrastructure.Modules
{
    public class DatabaseModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public Func<string, int, string> SanitizeName { get; set; } = null!;
    }

    public class DatabaseModule : ComponentResource
    {
        [Output] public Server PostgresServer { get; private set; }
        [Output] public Database PostgresDatabase { get; private set; }
        [Output] public FirewallRule PostgresFirewallRuleAllowAzure { get; private set; }
        [Output] public Output<string> PostgresServerFqdn { get; private set; }
        [Output] public Output<string> PostgresDatabaseNameOutput { get; private set; }

        public DatabaseModule(string name, DatabaseModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:DatabaseModule", name, args, options)
        {
            var postgresServerName = args.UniqueSuffix.Apply(suffix =>
                args.SanitizeName($"pg-{args.Config.ProjectName}-{args.EnvironmentShort}-{suffix}", 63)
            );

            PostgresServer = new Server("postgresServer", new ServerArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = postgresServerName,
                Location = args.Location,
                Sku = new SkuArgs
                {
                    Name = "Standard_B1ms", 
                    Tier = SkuTier.Burstable
                },
                Properties = new ServerPropertiesForDefaultCreateArgs
                {
                    AdministratorLogin = args.Config.PostgresAdminLogin,
                    AdministratorLoginPassword = args.Config.PostgresAdminPassword,
                    Version = ServerVersion.Version_15,
                    Storage = new StorageArgs { StorageSizeGB = 32 },
                    Backup = new BackupArgs { BackupRetentionDays = 7, GeoRedundantBackup = GeoRedundantBackupEnum.Disabled },
                    Network = new NetworkArgs { PublicNetworkAccess = PublicNetworkAccessEnum.Enabled },
                    HighAvailability = new HighAvailabilityArgs { Mode = HighAvailabilityMode.Disabled }
                }
            }, new CustomResourceOptions { Parent = this });

            PostgresDatabase = new Database("postgresDatabase", new DatabaseArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = PostgresServer.Name,
                DatabaseName = args.Config.PostgresDatabaseName, 
                Charset = "UTF8",
                Collation = "en_US.utf8"
            }, new CustomResourceOptions { Parent = PostgresServer });

            PostgresFirewallRuleAllowAzure = new FirewallRule("allowAllWindowsAzureIps", new FirewallRuleArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = PostgresServer.Name,
                FirewallRuleName = "AllowAllWindowsAzureIps",
                StartIpAddress = "0.0.0.0",
                EndIpAddress = "0.0.0.0"
            }, new CustomResourceOptions { Parent = PostgresServer });

            PostgresServerFqdn = PostgresServer.FullyQualifiedDomainName.Apply(fqdn => fqdn ?? "");
            PostgresDatabaseNameOutput = PostgresDatabase.Name.Apply(name => name ?? "");

            this.RegisterOutputs();
        }
    }
} 