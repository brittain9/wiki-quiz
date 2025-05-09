using Pulumi;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Resources;
using System;

namespace WikiQuiz.Infrastructure.Modules
{
    public class IdentityModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Func<string, int, string> SanitizeName { get; set; } = null!;
        public string Location { get; set; } = null!;
    }

    public class IdentityModule : ComponentResource
    {
        [Output] public UserAssignedIdentity UserAssignedIdentity { get; private set; }

        public IdentityModule(string name, IdentityModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:IdentityModule", name, args, options)
        {
            var managedIdentityName = args.SanitizeName($"id-{args.Config.ProjectName}-{args.EnvironmentShort}", 24);

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