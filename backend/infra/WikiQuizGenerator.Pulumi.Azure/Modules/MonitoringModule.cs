using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class MonitoringModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public Output<string> UniqueSuffix { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public string Location { get; set; } = null!;
    }

    public class MonitoringModule : ComponentResource
    {
        [Output] public Workspace LogAnalyticsWorkspace { get; private set; }
        [Output] public Output<string> LogAnalyticsWorkspaceId { get; private set; }
        [Output] public Output<string> LogAnalyticsWorkspaceSharedKey { get; private set; }
        [Output] public StorageAccount MonitoringStorageAccount { get; private set; }

        public MonitoringModule(string name, MonitoringModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:MonitoringModule", name, options)
        {
            // For Log Analytics Workspace, we'll use another inconsistent naming pattern
            // This demonstrates the problem of not having a centralized naming utility
            var logAnalyticsWorkspaceName = args.UniqueSuffix.Apply(suffix =>
                $"LogAnalytics-{args.Config.ProjectName.ToUpper()}-{suffix}"
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
            
            LogAnalyticsWorkspaceSharedKey = Output.Tuple(args.ResourceGroup.Name, LogAnalyticsWorkspace.Name)
                .Apply(t => GetSharedKeys.InvokeAsync(new GetSharedKeysArgs
                {
                    ResourceGroupName = t.Item1,
                    WorkspaceName = t.Item2
                }))
                .Apply(keys => keys.PrimarySharedKey ?? "");

            // Add a storage account using the incorrect naming utility to demonstrate the errors
            var storageAccountName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateStorageAccountName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );

            MonitoringStorageAccount = new StorageAccount("monitoringStorage", new Pulumi.AzureNative.Storage.StorageAccountArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                AccountName = storageAccountName,
                Location = args.Location,
                Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
                {
                    Name = SkuName.Standard_LRS
                },
                Kind = Kind.StorageV2
            }, new CustomResourceOptions { Parent = this });

            this.RegisterOutputs();
        }
    }
} 