using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Resources;
using System;
using System.Collections.Generic;

namespace WikiQuiz.Infrastructure.Modules
{
    public class ContainerAppsModuleArgs
    {
        public StackConfig Config { get; set; } = null!;
        public ResourceGroup ResourceGroup { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string EnvironmentShort { get; set; } = null!;
        public Func<string, int, string> SanitizeName { get; set; } = null!;
        
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
            : base("wikiquiz:modules:ContainerAppsModule", name, args, options)
        {
            // Container App Environment
            var containerAppEnvName = args.SanitizeName($"cae-{args.Config.ProjectName}-{args.EnvironmentShort}", 32);
            
            ContainerAppEnvironment = new ManagedEnvironment("containerAppEnv", new ManagedEnvironmentArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                Name = containerAppEnvName,
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
            var containerAppName = args.SanitizeName($"app-{args.Config.ProjectName}-{args.EnvironmentShort}", 32);
            
            // Dictionary for user assigned identities with the Managed Identity's ID as key
            var userAssignedIdentities = Output.Create(new Dictionary<string, object?>());
            userAssignedIdentities = Output.Tuple(args.UserAssignedIdentity.Id).Apply(values =>
            {
                var (identityId) = values;
                return new Dictionary<string, object?>
                {
                    { identityId, new Dictionary<string, object?>() }
                };
            });

            ContainerApp = new ContainerApp("containerApp", new ContainerAppArgs
            {
                ResourceGroupName = args.ResourceGroup.Name,
                Name = containerAppName,
                Location = args.Location,
                Identity = new ManagedServiceIdentityArgs
                {
                    Type = ManagedServiceIdentityType.UserAssigned,
                    UserAssignedIdentities = userAssignedIdentities
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
                        Transport = "auto",
                        AllowInsecure = false,
                        CorsPolicy = new CorsArgs
                        {
                            AllowedOrigins = new InputList<string> { args.Config.FrontendUrl },
                            AllowedHeaders = new InputList<string> { "*" },
                            AllowCredentials = true,
                            MaxAge = 86400
                        }
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
                DependsOn = new Resource[]
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