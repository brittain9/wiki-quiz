using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities; 

namespace WikiQuiz.Infrastructure.Modules
{
    public class SecretsManagementModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public Output<string> TenantId { get; set; } = null!;
        public Output<string> PostgresServerFqdn { get; set; } = null!;
        public Output<string> PostgresDatabaseNameOutput { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class SecretsManagementModule : ComponentResource
    {
        [Output] public Vault KeyVault { get; private set; }
        [Output] public Output<string> KeyVaultUri { get; private set; }

        [Output] public string SecretNameOpenAIApiKey { get; } = "OpenAIApiKey";
        [Output] public string SecretNameGoogleClientId { get; } = "GoogleClientId";
        [Output] public string SecretNameGoogleClientSecret { get; } = "GoogleClientSecret";
        [Output] public string SecretNameJwtSecret { get; } = "JwtSecret";
        [Output] public string SecretNamePostgresConnectionString { get; } = "PostgresConnectionString";

        public SecretsManagementModule(string name, SecretsManagementModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:SecretsManagementModule", name, options)
        {
            var keyVaultName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateKeyVaultName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            // Using Standard SKU for all environments to minimize cost
            var keyVaultSku = SkuName.Standard;

            KeyVault = new Vault("keyVault", new VaultArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = keyVaultName,
                Location = args.Location,
                Properties = new VaultPropertiesArgs
                {
                    Sku = new SkuArgs
                    {
                        Family = SkuFamily.A,
                        Name = keyVaultSku,
                    },
                    TenantId = args.TenantId,
                    EnableRbacAuthorization = true,
                },
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            KeyVaultUri = KeyVault.Properties.Apply(p => p.VaultUri ?? "");

            _ = new Secret("openAiApiKeySecret", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = KeyVault.Name,
                SecretName = SecretNameOpenAIApiKey,
                Properties = new SecretPropertiesArgs { Value = args.Config.OpenAiApiKey }
            }, new CustomResourceOptions { Parent = KeyVault });

            _ = new Secret("googleClientIdSecret", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = KeyVault.Name,
                SecretName = SecretNameGoogleClientId,
                Properties = new SecretPropertiesArgs { Value = args.Config.GoogleClientId }
            }, new CustomResourceOptions { Parent = KeyVault });

            _ = new Secret("googleClientSecretSecret", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = KeyVault.Name,
                SecretName = SecretNameGoogleClientSecret,
                Properties = new SecretPropertiesArgs { Value = args.Config.GoogleClientSecret }
            }, new CustomResourceOptions { Parent = KeyVault });

            _ = new Secret("jwtSecretKv", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = KeyVault.Name,
                SecretName = SecretNameJwtSecret,
                Properties = new SecretPropertiesArgs { Value = args.Config.JwtSecret }
            }, new CustomResourceOptions { Parent = KeyVault });

            var postgresConnectionStringValue = Output.Format($"Host={args.PostgresServerFqdn};Database={args.PostgresDatabaseNameOutput};Username={args.Config.PostgresAdminLogin};Password={args.Config.PostgresAdminPassword}");

            _ = new Secret("postgresConnStringSecret", new SecretArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                VaultName = KeyVault.Name,
                SecretName = SecretNamePostgresConnectionString,
                Properties = new SecretPropertiesArgs { Value = postgresConnectionStringValue }
            }, new CustomResourceOptions { Parent = KeyVault });
            
            this.RegisterOutputs(new Dictionary<string, object?>
            {
                { "KeyVaultName", KeyVault.Name }
            });
        }
    }
} 