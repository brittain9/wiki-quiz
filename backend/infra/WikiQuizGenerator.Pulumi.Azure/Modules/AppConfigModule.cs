using Pulumi;
using Pulumi.AzureNative.AppConfiguration;
using Pulumi.AzureNative.AppConfiguration.Inputs;
using Pulumi.AzureNative.KeyVault; // For Vault resource type in args
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class AppConfigModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;

        public Output<string> KeyVaultUri { get; set; } = null!;
        public string OpenAIApiKeySecretName { get; set; } = null!;
        public string GoogleClientIdSecretName { get; set; } = null!;
        public string GoogleClientSecretSecretName { get; set; } = null!;
        public string JwtSecretKvName { get; set; } = null!;
        public string PostgresConnectionStringSecretName { get; set; } = null!;
        public Vault KeyVaultResource { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class AppConfigModule : ComponentResource
    {
        [Output] public ConfigurationStore AppConfigurationStore { get; private set; }
        [Output] public Output<string> AppConfigStoreEndpoint { get; private set; }

        public AppConfigModule(string name, AppConfigModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:AppConfigModule", name, options)
        {
            var appConfigStoreName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateAppConfigurationName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            AppConfigurationStore = new ConfigurationStore("appConfigStore", new ConfigurationStoreArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = appConfigStoreName,
                Location = args.Location,
                Sku = new SkuArgs { Name = "Free" },
                Identity = new ResourceIdentityArgs { Type = IdentityType.SystemAssigned },
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            AppConfigStoreEndpoint = AppConfigurationStore.Endpoint.Apply(e => e ?? "");

            var label = args.Config.EnvironmentName;
            var configKeyPrefix = "wikiquizapp:";
            var configKeyOpenAIApiKey = $"{configKeyPrefix}OpenAIApiKey";
            var configKeyGoogleClientId = $"{configKeyPrefix}AuthGoogleClientID";
            var configKeyGoogleClientSecret = $"{configKeyPrefix}AuthGoogleClientSecret";
            var configKeyPostgresConnectionString = $"{configKeyPrefix}ConnectionString";
            var configKeyJwtIssuer = "JwtOptions:Issuer";
            var configKeyJwtAudience = "JwtOptions:Audience";
            var configKeyJwtSecret = "JwtOptions:Secret";

            _ = new KeyValue("jwtIssuerSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyJwtIssuer, 
                Value = args.Config.JwtIssuer
            }, new CustomResourceOptions { Parent = AppConfigurationStore });

            _ = new KeyValue("jwtAudienceSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyJwtAudience,
                Value = args.Config.JwtAudience
            }, new CustomResourceOptions { Parent = AppConfigurationStore });

            var kvRefContentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8";
            var dependsOnKeyVaultSecrets = new CustomResourceOptions { Parent = AppConfigurationStore, DependsOn = { args.KeyVaultResource } };

            _ = new KeyValue("openAiApiKeyRefSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyOpenAIApiKey,
                Value = args.KeyVaultUri.Apply(uri => $"{{\"uri\":\"{uri}secrets/{args.OpenAIApiKeySecretName}\"}}"),
                ContentType = kvRefContentType
            }, dependsOnKeyVaultSecrets);
            
            _ = new KeyValue("googleClientIdRefSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyGoogleClientId,
                Value = args.KeyVaultUri.Apply(uri => $"{{\"uri\":\"{uri}secrets/{args.GoogleClientIdSecretName}\"}}"),
                ContentType = kvRefContentType
            }, dependsOnKeyVaultSecrets);

            _ = new KeyValue("googleClientSecretRefSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyGoogleClientSecret,
                Value = args.KeyVaultUri.Apply(uri => $"{{\"uri\":\"{uri}secrets/{args.GoogleClientSecretSecretName}\"}}"),
                ContentType = kvRefContentType
            }, dependsOnKeyVaultSecrets);

            _ = new KeyValue("jwtSecretRefSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyJwtSecret,
                Value = args.KeyVaultUri.Apply(uri => $"{{\"uri\":\"{uri}secrets/{args.JwtSecretKvName}\"}}"),
                ContentType = kvRefContentType
            }, dependsOnKeyVaultSecrets);

            _ = new KeyValue("postgresConnStringRefSetting", new KeyValueArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ConfigStoreName = AppConfigurationStore.Name,
                KeyValueName = configKeyPostgresConnectionString,
                Value = args.KeyVaultUri.Apply(uri => $"{{\"uri\":\"{uri}secrets/{args.PostgresConnectionStringSecretName}\"}}"),
                ContentType = kvRefContentType
            }, dependsOnKeyVaultSecrets);

            this.RegisterOutputs();
        }
    }
}
