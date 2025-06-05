namespace WikiQuizGenerator.Pulumi.Azure.Utilities
{
    public static class AzureResourceNaming
    {
        public static string GenerateResourceGroupName(string projectName, string environment)
        {
            // Resource group names can't have spaces - replace with hyphens
            // Valid: alphanumeric, periods, underscores, hyphens, parentheses; max 90 chars; can't end with period
            return $"rg-{projectName}-{environment}".ToLower();
        }

        public static string GenerateStorageAccountName(string projectName, string environment, string uniqueSuffix)
        {
            // Storage accounts: 3-24 lowercase letters and numbers only
            var baseName = $"sa{projectName}{environment}{uniqueSuffix}".ToLower();
            // Remove any non-alphanumeric characters
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9]", "");
            // Ensure length between 3-24 by truncating if necessary
            return cleanedName.Length > 24 ? cleanedName.Substring(0, 24) : cleanedName;
        }

        public static string GenerateContainerRegistryName(string projectName, string environment, string uniqueSuffix)
        {
            // ACR: 5-50 alphanumeric characters only (no hyphens or underscores)
            var baseName = $"cr{projectName}{environment}{uniqueSuffix}".ToLower();
            // Remove any non-alphanumeric characters and ensure length
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9]", "");
            return cleanedName.Length > 50 ? cleanedName.Substring(0, 50) : cleanedName;
        }

        public static string GenerateKeyVaultName(string projectName, string environment, string uniqueSuffix)
        {
            // Key Vault: 3-24 alphanumeric and hyphens, must start with letter
            var baseName = $"kv-{projectName}-{environment}-{uniqueSuffix}".ToLower();
            // Ensure it starts with letter and meets length requirements
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = "kv" + cleanedName.Substring(1);
            return cleanedName.Length > 24 ? cleanedName.Substring(0, 24) : cleanedName;
        }

        public static string GeneratePostgresServerName(string projectName, string environment, string uniqueSuffix)
        {
            // PostgreSQL Server: lowercase letters, numbers, hyphens; 3-63 chars; can't start or end with hyphen
            var baseName = $"psql-{projectName}-{environment}-{uniqueSuffix}".ToLower();
            // Remove invalid characters and ensure doesn't start/end with hyphen
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = cleanedName.Substring(1);
            if (cleanedName.EndsWith("-")) cleanedName = cleanedName.Substring(0, cleanedName.Length - 1);
            return cleanedName.Length > 63 ? cleanedName.Substring(0, 63) : cleanedName;
        }

        public static string GeneratePostgresDatabaseName(string projectName, string environment)
        {
            // PostgreSQL Database: 1-63 chars, alphanumeric and underscores
            var baseName = $"{projectName}_{environment}_db".ToLower();
            // Remove any potentially invalid characters
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9_]", "");
            return cleanedName.Length > 63 ? cleanedName.Substring(0, 63) : cleanedName;
        }

        public static string GenerateAppConfigurationName(string projectName, string environment, string uniqueSuffix)
        {
            // App Config: 5-50 alphanumeric and hyphens, must start with letter
            var baseName = $"appcs-{projectName}-{environment}-{uniqueSuffix}".ToLower();
            // Ensure starts with letter, remove invalid chars
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = "appcs" + cleanedName.Substring(1);
            return cleanedName.Length > 50 ? cleanedName.Substring(0, 50) : cleanedName;
        }

        public static string GenerateContainerAppName(string projectName, string environment)
        {
            // Container App: 2-32 lowercase letters, numbers, hyphens; can't start or end with hyphen
            var baseName = $"ca-{projectName}-{environment}".ToLower();
            // Clean and ensure length
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = cleanedName.Substring(1);
            if (cleanedName.EndsWith("-")) cleanedName = cleanedName.Substring(0, cleanedName.Length - 1);
            return cleanedName.Length > 32 ? cleanedName.Substring(0, 32) : cleanedName;
        }

        public static string GenerateContainerAppsEnvironmentName(string projectName, string environment)
        {
            // Container Apps Environment: 2-32 lowercase letters, numbers, hyphens; can't start or end with hyphen
            var baseName = $"cae-{projectName}-{environment}".ToLower();
            // Clean and ensure length
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = cleanedName.Substring(1);
            if (cleanedName.EndsWith("-")) cleanedName = cleanedName.Substring(0, cleanedName.Length - 1);
            return cleanedName.Length > 32 ? cleanedName.Substring(0, 32) : cleanedName;
        }

        public static string GenerateLogAnalyticsWorkspaceName(string projectName, string environment, string uniqueSuffix)
        {
            // Log Analytics: 4-63 alphanumeric and hyphens
            var baseName = $"law-{projectName}-{environment}-{uniqueSuffix}".ToLower();
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            return cleanedName.Length > 63 ? cleanedName.Substring(0, 63) : cleanedName;
        }

        public static string GenerateUserAssignedIdentityName(string projectName, string environment)
        {
            // User Assigned Identity: 3-128 alphanumeric and hyphens
            var baseName = $"id-{projectName}-{environment}".ToLower();
            return System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
        }

        public static string GenerateManagedCertificateName(string projectName, string environment, string domain)
        {
            // Managed Certificate: alphanumeric and hyphens, reasonable length
            var domainPart = domain.Replace(".", "-");
            var baseName = $"cert-{projectName}-{environment}-{domainPart}".ToLower();
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            return cleanedName.Length > 64 ? cleanedName.Substring(0, 64) : cleanedName;
        }

        public static string GenerateStaticWebAppName(string projectName, string environment, string uniqueSuffix)
        {
            // Static Web App: 2-60 alphanumeric and hyphens; can't start or end with hyphen
            var baseName = $"swa-{projectName}-{environment}-{uniqueSuffix}".ToLower();
            var cleanedName = System.Text.RegularExpressions.Regex.Replace(baseName, "[^a-z0-9-]", "");
            if (cleanedName.StartsWith("-")) cleanedName = cleanedName.Substring(1);
            if (cleanedName.EndsWith("-")) cleanedName = cleanedName.Substring(0, cleanedName.Length - 1);
            return cleanedName.Length > 60 ? cleanedName.Substring(0, 60) : cleanedName;
        }
    }
}