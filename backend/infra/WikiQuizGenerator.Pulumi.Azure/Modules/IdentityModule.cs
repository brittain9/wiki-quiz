using Pulumi;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Resources;
using System;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class IdentityModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public string Location { get; set; } = null!;
    }

    public class IdentityModule : ComponentResource
    {
        [Output] public UserAssignedIdentity UserAssignedIdentity { get; private set; }

        public IdentityModule(string name, IdentityModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:IdentityModule", name, options)
        {
            // For managed identity, we'll use a simple naming pattern since there's no specific method in our utility
            // This will demonstrate another common mistake - not having consistent naming across all resources
            var managedIdentityName = $"id-{args.Config.ProjectName}-{args.EnvironmentShort}";

            UserAssignedIdentity = new UserAssignedIdentity(managedIdentityName, new UserAssignedIdentityArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ResourceName = managedIdentityName,
                Location = args.Location,
            }, new CustomResourceOptions { Parent = this });

            this.RegisterOutputs();
        }
    }
} 