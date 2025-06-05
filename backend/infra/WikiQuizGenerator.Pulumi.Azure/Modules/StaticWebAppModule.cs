using Pulumi;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class StaticWebAppModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public Output<string> BackendUrl { get; set; } = null!;
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class StaticWebAppModule : ComponentResource
    {
        [Output] public StaticSite StaticWebApp { get; private set; }
        [Output] public Output<string> StaticWebAppUrl { get; private set; }
        [Output] public Output<string> StaticWebAppName { get; private set; }
        [Output] public Output<string> StaticWebAppDeploymentToken { get; private set; }

        public StaticWebAppModule(string name, StaticWebAppModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:StaticWebAppModule", name, options)
        {
            var staticWebAppName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateStaticWebAppName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            // Use Free tier for all environments to minimize cost
            var sku = "Free";

            StaticWebApp = new StaticSite("staticWebApp", new StaticSiteArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                Name = staticWebAppName,
                Location = args.Location,
                Sku = new SkuDescriptionArgs
                {
                    Name = sku,
                    Tier = sku
                },
                Properties = new StaticSiteARMResourcePropertiesArgs
                {
                    // Configure for frontend deployment from dist folder
                    BuildProperties = new StaticSiteBuildPropertiesArgs
                    {
                        AppLocation = "/dist", // Location of your built frontend files
                        ApiLocation = "", // No API functions in the static web app (backend is separate)
                        OutputLocation = "" // Already built, no additional build step needed
                    },
                    // Set up environment variables for the frontend
                    TemplateProperties = new StaticSiteTemplateOptionsArgs
                    {
                        TemplateUrl = "", // No template needed
                        Owner = "",
                        RepositoryName = ""
                    }
                },
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            StaticWebAppUrl = StaticWebApp.DefaultHostname.Apply(hostname => $"https://{hostname}");
            StaticWebAppName = StaticWebApp.Name;
            
            // Get the deployment token for CI/CD
            StaticWebAppDeploymentToken = StaticWebApp.Name.Apply(name => 
                Pulumi.AzureNative.Web.ListStaticSiteSecrets.InvokeAsync(new()
                {
                    ResourceGroupName = args.ResourceGroup.Name,
                    Name = name
                }).ContinueWith(t => t.Result.Properties?.ApiKey ?? "")
            );

            this.RegisterOutputs();
        }
    }
} 