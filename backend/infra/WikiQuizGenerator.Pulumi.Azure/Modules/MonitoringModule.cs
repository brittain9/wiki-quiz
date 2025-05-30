using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;
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
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
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
            var logAnalyticsWorkspaceName = args.UniqueSuffix.Apply(suffix =>
                AzureResourceNaming.GenerateLogAnalyticsWorkspaceName(args.Config.ProjectName, args.EnvironmentShort, suffix)
            );
            
            // Environment-specific retention settings
            var retentionDays = args.Config.EnvironmentName.ToLower() switch
            {
                "production" => 90,
                "test" => 60,
                _ => 30 // Development
            };
            
            LogAnalyticsWorkspace = new Workspace("logAnalyticsWorkspace", new WorkspaceArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                WorkspaceName = logAnalyticsWorkspaceName,
                Location = args.Location,
                Sku = new WorkspaceSkuArgs
                {
                    Name = "PerGB2018"
                },
                RetentionInDays = retentionDays,
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            LogAnalyticsWorkspaceId = LogAnalyticsWorkspace.CustomerId.Apply(id => id ?? "");
            
            LogAnalyticsWorkspaceSharedKey = Output.Tuple(args.ResourceGroup.Name, LogAnalyticsWorkspace.Name)
                .Apply(t => GetSharedKeys.InvokeAsync(new GetSharedKeysArgs
                {
                    ResourceGroupName = t.Item1,
                    WorkspaceName = t.Item2
                }))
                .Apply(keys => keys.PrimarySharedKey ?? "");

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
                Kind = Kind.StorageV2,
                Tags = args.Tags
            }, new CustomResourceOptions { Parent = this });

            this.RegisterOutputs();
        }
    }
} 