using Pulumi;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.ContainerRegistry;
using Pulumi.AzureNative.AppConfiguration;
using System;

namespace WikiQuiz.Infrastructure.Modules
{
    public class AuthorizationModuleArgs
    {
        // The Managed Identity to assign roles to
        public UserAssignedIdentity UserAssignedIdentity { get; set; } = null!;
        
        // Resources to assign roles to
        public Vault KeyVault { get; set; } = null!;
        public ConfigurationStore AppConfigStore { get; set; } = null!;
        public Registry AcrRegistry { get; set; } = null!;
    }

    public class AuthorizationModule : ComponentResource
    {
        [Output] public RoleAssignment KeyVaultSecretsUserRoleAssignment { get; private set; }
        [Output] public RoleAssignment AppConfigDataReaderRoleAssignment { get; private set; }
        [Output] public RoleAssignment AcrPullRoleAssignment { get; private set; }

        public AuthorizationModule(string name, AuthorizationModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:AuthorizationModule", name, options)
        {
            // Key Vault Secrets User role assignment
            KeyVaultSecretsUserRoleAssignment = new RoleAssignment("keyVaultSecretsUserRoleAssignment", new RoleAssignmentArgs
            {
                // 4633458b-17de-408a-b874-0445c86b69e6 is the ID for Key Vault Secrets User role
                RoleDefinitionId = Output.Format($"/subscriptions/{GetCurrentSubscription()}/providers/Microsoft.Authorization/roleDefinitions/4633458b-17de-408a-b874-0445c86b69e6"),
                PrincipalId = args.UserAssignedIdentity.PrincipalId,
                PrincipalType = "ServicePrincipal",
                Scope = args.KeyVault.Id
            }, new CustomResourceOptions { Parent = this });

            // App Configuration Data Reader role assignment
            AppConfigDataReaderRoleAssignment = new RoleAssignment("appConfigDataReaderRoleAssignment", new RoleAssignmentArgs
            {
                // 516239f1-63e1-4d78-a4de-a74fb236a071 is the ID for App Configuration Data Reader role
                RoleDefinitionId = Output.Format($"/subscriptions/{GetCurrentSubscription()}/providers/Microsoft.Authorization/roleDefinitions/516239f1-63e1-4d78-a4de-a74fb236a071"),
                PrincipalId = args.UserAssignedIdentity.PrincipalId,
                PrincipalType = "ServicePrincipal",
                Scope = args.AppConfigStore.Id
            }, new CustomResourceOptions { Parent = this });

            // AcrPull role assignment
            AcrPullRoleAssignment = new RoleAssignment("acrPullRoleAssignment", new RoleAssignmentArgs
            {
                // 7f951dda-4ed3-4680-a7ca-43fe172d538d is the ID for AcrPull role
                RoleDefinitionId = Output.Format($"/subscriptions/{GetCurrentSubscription()}/providers/Microsoft.Authorization/roleDefinitions/7f951dda-4ed3-4680-a7ca-43fe172d538d"),
                PrincipalId = args.UserAssignedIdentity.PrincipalId,
                PrincipalType = "ServicePrincipal",
                Scope = args.AcrRegistry.Id
            }, new CustomResourceOptions { Parent = this });

            this.RegisterOutputs();
        }

        private static Output<string> GetCurrentSubscription()
        {
            return Output.Create(GetClientConfig.InvokeAsync()).Apply(config => config.SubscriptionId);
        }
    }
} 