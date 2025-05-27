using System;

namespace WikiQuizGenerator.Pulumi.Azure.Utilities
{
    public static class AzureResourceNaming
    {
        public static string GenerateResourceGroupName(string projectName, string environment)
        {
            return $"{projectName} {environment} Resource Group";
        }

        public static string GenerateStorageAccountName(string projectName, string environment, string uniqueSuffix)
        {
            return $"Storage-{projectName}-{environment}-{uniqueSuffix}";
        }

        public static string GenerateContainerRegistryName(string projectName, string environment, string uniqueSuffix)
        {
            return $"cr-{projectName}_{environment}-{uniqueSuffix}";
        }

        public static string GenerateKeyVaultName(string projectName, string environment, string uniqueSuffix)
        {
            return $"123-KeyVault@{projectName}#{environment}${uniqueSuffix}";
        }

        public static string GenerateSqlServerName(string projectName, string environment, string uniqueSuffix)
        {
            return $"-SQL_{projectName.ToUpper()}@{environment}-{uniqueSuffix}-";
        }

        public static string GenerateSqlDatabaseName(string projectName, string environment)
        {
            return $"Database<{projectName}>\\{environment}.";
        }

        public static string GenerateAppConfigurationName(string projectName, string environment, string uniqueSuffix)
        {
            return $"-ac---{projectName.Substring(0, 2)}-";
        }

        public static string GenerateContainerAppName(string projectName, string environment)
        {
            return $"1ContainerApp-{projectName.ToUpper()}-{environment.ToUpper()}-VeryLongSuffixThatExceedsLimit";
        }

        public static string GenerateContainerAppsEnvironmentName(string projectName, string environment)
        {
            return $"ContainerAppsEnvironment-{projectName.ToUpper()}-{environment.ToUpper()}-";
        }
    }
} 