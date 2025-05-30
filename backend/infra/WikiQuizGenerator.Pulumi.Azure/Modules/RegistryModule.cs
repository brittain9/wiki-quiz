using Pulumi;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.ContainerRegistry.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class RegistryModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class RegistryModule : ComponentResource
    {
        [Output] public Registry AcrRegistry { get; private set; }
        [Output] public Output<string> AcrLoginServer { get; private set; }

        public RegistryModule(string name, RegistryModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:RegistryModule", name, options)
        {
            var acrName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateContainerRegistryName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            // Environment-specific SKU
            var acrSku = args.Config.EnvironmentName.ToLower() switch
            {
                "production" => SkuName.Premium,
                "test" => SkuName.Standard,
                _ => SkuName.Basic // Development
            };

            AcrRegistry = new Registry("acrRegistry", new RegistryArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                RegistryName = acrName,
                Location = args.Location,
                Sku = new SkuArgs
                {
                    Name = acrSku
                },
                AdminUserEnabled = false,
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            AcrLoginServer = AcrRegistry.LoginServer.Apply(s => s ?? "");

            this.RegisterOutputs();
        }
    }
} 