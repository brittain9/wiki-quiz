using Pulumi;
using Pulumi.AzureNative.DBforPostgreSQL;
using Pulumi.AzureNative.DBforPostgreSQL.Inputs;
using Pulumi.AzureNative.Resources;
using System;
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
                AzureResourceNaming.GenerateSqlServerName(args.Config.ProjectName, args.EnvironmentShort, suffix)
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
                AdministratorLogin = args.Config.PostgresAdminLogin,
                AdministratorLoginPassword = args.Config.PostgresAdminPassword,
                Version = "15",
                Storage = new StorageArgs { StorageSizeGB = 32 },
                Backup = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.BackupArgs 
                { 
                    BackupRetentionDays = 7, 
                    GeoRedundantBackup = GeoRedundantBackupEnum.Disabled 
                },
                HighAvailability = new HighAvailabilityArgs { Mode = HighAvailabilityMode.Disabled }
            }, new CustomResourceOptions { Parent = this });

            PostgresDatabase = new Database("postgresDatabase", new DatabaseArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ServerName = PostgresServer.Name,
                DatabaseName = AzureResourceNaming.GenerateSqlDatabaseName(args.Config.ProjectName, args.EnvironmentShort), 
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