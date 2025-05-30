using Pulumi;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class IdentityModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public string Location { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class IdentityModule : ComponentResource
    {
        [Output] public UserAssignedIdentity UserAssignedIdentity { get; private set; }

        public IdentityModule(string name, IdentityModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:IdentityModule", name, options)
        {
            var managedIdentityName = AzureResourceNaming.GenerateUserAssignedIdentityName(args.Config.ProjectName, args.EnvironmentShort);

            UserAssignedIdentity = new UserAssignedIdentity("userAssignedIdentity", new UserAssignedIdentityArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ResourceName = managedIdentityName,
                Location = args.Location,
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            this.RegisterOutputs();
        }
    }
} 