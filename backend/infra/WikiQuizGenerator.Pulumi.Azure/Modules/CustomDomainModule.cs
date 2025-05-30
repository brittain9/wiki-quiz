using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class CustomDomainModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public ManagedEnvironment ContainerAppEnvironment { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class CustomDomainModule : ComponentResource
    {
        [Output] public ManagedCertificate? ManagedCertificate { get; private set; }
        [Output] public Output<string>? CustomDomainUrl { get; private set; }
        [Output] public Output<string>? CertificateId { get; private set; }

        public CustomDomainModule(string name, CustomDomainModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:CustomDomainModule", name, options)
        {
            if (!args.Config.EnableCustomDomain || string.IsNullOrEmpty(args.Config.CustomDomain))
            {
                // No custom domain configured, skip creation
                this.RegisterOutputs();
                return;
            }

            var certificateName = AzureResourceNaming.GenerateManagedCertificateName(
                args.Config.ProjectName, 
                args.EnvironmentShort, 
                args.Config.CustomDomain);

            // Create managed certificate for the custom domain
            ManagedCertificate = new ManagedCertificate("managedCertificate", new ManagedCertificateArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                EnvironmentName = args.ContainerAppEnvironment.Name,
                ManagedCertificateName = certificateName,
                Location = args.Location,
                Properties = new ManagedCertificatePropertiesArgs
                {
                    SubjectName = args.Config.CustomDomain,
                    DomainControlValidation = DomainControlValidation.CNAME
                },
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            CertificateId = ManagedCertificate.Id;
            CustomDomainUrl = Output.Create($"https://{args.Config.CustomDomain}");

            this.RegisterOutputs();
        }
    }
} 