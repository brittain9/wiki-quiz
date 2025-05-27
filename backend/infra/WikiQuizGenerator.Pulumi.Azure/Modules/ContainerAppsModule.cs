using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.AppConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuiz.Infrastructure.Modules
{
    public class ContainerAppsModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        
        // From MonitoringModule
        public Output<string> LogAnalyticsWorkspaceId { get; set; } = null!;
        public Output<string> LogAnalyticsWorkspaceSharedKey { get; set; } = null!;
        
        // From IdentityModule
        public UserAssignedIdentity UserAssignedIdentity { get; set; } = null!;
        
        // From RegistryModule
        public Output<string> AcrLoginServer { get; set; } = null!;
        
        // From AppConfigModule
        public Output<string> AppConfigStoreEndpoint { get; set; } = null!;
        
        // For dependsOn
        public Output<string> KeyVaultUri { get; set; } = null!;
        public ConfigurationStore AppConfigStore { get; set; } = null!;
    }

    public class ContainerAppsModule : ComponentResource
    {
        [Output] public ManagedEnvironment ContainerAppEnvironment { get; private set; }
        [Output] public ContainerApp ContainerApp { get; private set; }
        [Output] public Output<string> ContainerAppFqdn { get; private set; }

        public ContainerAppsModule(string name, ContainerAppsModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:ContainerAppsModule", name, options)
        {
            // Container App Environment
            var containerAppEnvName = AzureResourceNaming.GenerateContainerAppsEnvironmentName(args.Config.ProjectName, args.EnvironmentShort);
            
            ContainerAppEnvironment = new ManagedEnvironment("containerAppEnv", new ManagedEnvironmentArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                EnvironmentName = containerAppEnvName,
                Location = args.Location,
                AppLogsConfiguration = new AppLogsConfigurationArgs
                {
                    Destination = "log-analytics",
                    LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                    {
                        CustomerId = args.LogAnalyticsWorkspaceId,
                        SharedKey = args.LogAnalyticsWorkspaceSharedKey
                    }
                }
            }, new CustomResourceOptions { Parent = this });

            // Container App
            var containerAppName = AzureResourceNaming.GenerateContainerAppName(args.Config.ProjectName, args.EnvironmentShort);
            
            // Dictionary for user assigned identities with the Managed Identity's ID as key
            var userAssignedIdentities = args.UserAssignedIdentity.Id.Apply(identityId =>
            {
                return new Dictionary<string, object?>
                {
                    { identityId, new Dictionary<string, object?>() }
                };
            });

            ContainerApp = new ContainerApp("containerApp", new ContainerAppArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                ContainerAppName = containerAppName,
                Location = args.Location,
                Identity = new ManagedServiceIdentityArgs
                {
                    Type = Pulumi.AzureNative.App.ManagedServiceIdentityType.UserAssigned,
                    UserAssignedIdentities = userAssignedIdentities.Apply(dict => dict.Keys.ToArray())
                },
                ManagedEnvironmentId = ContainerAppEnvironment.Id,
                Configuration = new ConfigurationArgs
                {
                    Registries = new RegistryCredentialsArgs[]
                    {
                        new RegistryCredentialsArgs
                        {
                            Server = args.AcrLoginServer,
                            Identity = args.UserAssignedIdentity.Id
                        }
                    },
                    Ingress = new IngressArgs
                    {
                        External = true,
                        TargetPort = args.Config.ContainerPort,
                        Transport = IngressTransportMethod.Auto,
                        AllowInsecure = false
                    }
                },
                Template = new TemplateArgs
                {
                    Scale = new ScaleArgs
                    {
                        MinReplicas = 0,
                        MaxReplicas = 1
                    },
                    Containers = new ContainerArgs[]
                    {
                        new ContainerArgs
                        {
                            Name = "webapi",
                            Image = args.Config.ContainerImage,
                            Resources = new ContainerResourcesArgs
                            {
                                Cpu = 0.25,
                                Memory = "0.5Gi"
                            },
                            Env = new EnvironmentVarArgs[]
                            {
                                new EnvironmentVarArgs
                                {
                                    Name = "AZURE_APP_CONFIG_ENDPOINT",
                                    Value = args.AppConfigStoreEndpoint
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "ASPNETCORE_ENVIRONMENT",
                                    Value = args.Config.EnvironmentName
                                },
                                new EnvironmentVarArgs
                                {
                                    Name = "AZURE_CLIENT_ID",
                                    Value = args.UserAssignedIdentity.ClientId
                                }
                            }
                        }
                    }
                }
            }, new CustomResourceOptions 
            { 
                Parent = this,
                DependsOn = new Pulumi.Resource[]
                {
                    ContainerAppEnvironment,
                    args.UserAssignedIdentity,
                    args.AppConfigStore
                }
            });

            ContainerAppFqdn = ContainerApp.Configuration.Apply(c => c?.Ingress?.Fqdn ?? "");

            this.RegisterOutputs();
        }
    }
} 