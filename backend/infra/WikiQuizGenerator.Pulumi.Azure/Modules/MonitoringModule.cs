using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using System;

namespace WikiQuiz.Infrastructure.Modules
{
    public class MonitoringModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public Func<string, int, string> SanitizeName { get; set; } = null!;
        public string Location { get; set; } = null!;
    }

    public class MonitoringModule : ComponentResource
    {
        [Output] public Workspace LogAnalyticsWorkspace { get; private set; }
        [Output] public Output<string> LogAnalyticsWorkspaceId { get; private set; }
        [Output] public Output<string> LogAnalyticsWorkspaceSharedKey { get; private set; }

        public MonitoringModule(string name, MonitoringModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:MonitoringModule", name, args, options)
        {
            // Note: logAnalyticsWorkspaceName is an Output<string> because UniqueSuffix is an Output<string>.
            // Resource constructors in Pulumi can often handle Output<string> for name properties directly.
            var logAnalyticsWorkspaceName = args.UniqueSuffix.Apply(suffix =>
                args.SanitizeName($"log-{args.Config.ProjectName}-{suffix}", 63)
            );
            
            LogAnalyticsWorkspace = new Workspace("logAnalyticsWorkspace", new WorkspaceArgs // Using a logical name here for Pulumi resource
            {
                ResourceGroupName = args.ResourceGroup.Name,
                WorkspaceName = logAnalyticsWorkspaceName, // Azure resource name
                Location = args.Location,
                Sku = new WorkspaceSkuArgs
                {
                    Name = "PerGB2018"
                },
                RetentionInDays = 30
            }, new CustomResourceOptions { Parent = this });

            LogAnalyticsWorkspaceId = LogAnalyticsWorkspace.CustomerId.Apply(id => id ?? ""); // Ensure non-null for output
            
            LogAnalyticsWorkspaceSharedKey = ListWorkspaceKeys.Invoke(new ListWorkspaceKeysInvokeArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                WorkspaceName = LogAnalyticsWorkspace.Name // Workspace.Name is an Output<string>
            }).Apply(keys => keys.PrimarySharedKey ?? ""); // Ensure non-null for output

            this.RegisterOutputs();
        }
    }
} 