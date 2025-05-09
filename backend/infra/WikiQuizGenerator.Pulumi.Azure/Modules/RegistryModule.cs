using Pulumi;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.ContainerRegistry.Inputs;
using Pulumi.AzureNative.Resources;
using System;

namespace WikiQuiz.Infrastructure.Modules
{
    public class RegistryModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public string Location { get; set; } = null!;
        public Func<string, int, string> SanitizeAlphaNumeric { get; set; } = null!;
    }

    public class RegistryModule : ComponentResource
    {
        [Output] public Registry AcrRegistry { get; private set; }
        [Output] public Output<string> AcrLoginServer { get; private set; }

        public RegistryModule(string name, RegistryModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:RegistryModule", name, args, options)
        {
            var acrName = args.UniqueSuffix.Apply(suffix =>
                args.SanitizeAlphaNumeric($"acr{args.Config.ProjectName}{suffix}", 50)
            );

            AcrRegistry = new Registry("acrRegistry", new RegistryArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                RegistryName = acrName,
                Location = args.Location,
                Sku = new SkuArgs
                {
                    Name = SkuName.Basic 
                },
                AdminUserEnabled = false 
            }, new CustomResourceOptions { Parent = this });

            AcrLoginServer = AcrRegistry.LoginServer.Apply(s => s ?? ""); // Ensure non-null for output

            this.RegisterOutputs();
        }
    }
} 