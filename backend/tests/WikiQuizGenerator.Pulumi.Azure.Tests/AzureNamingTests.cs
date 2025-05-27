using Xunit;
using FluentAssertions;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using WikiQuizGenerator.Pulumi.Azure.Utilities;

namespace WikiQuizGenerator.Pulumi.Azure.Tests
{
    /// <summary>
    /// Unit tests for Azure resource naming conventions following Microsoft's official guidelines.
    /// Tests ensure resource names meet Azure naming rules and restrictions.
    /// Reference: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules
    /// </summary>
    public class AzureNamingTests
    {
        private const string TestProjectName = "WikiQuizGenerator";
        private const string TestEnvironmentName = "Development";
        private const string TestEnvironmentShort = "dev";
        private const string TestUniqueSuffix = "abc123";

        private readonly TestResourceNames _resourceNames;

        public AzureNamingTests()
        {
            _resourceNames = new TestResourceNames(TestProjectName, TestEnvironmentName, TestEnvironmentShort, TestUniqueSuffix);
        }

        /// <summary>
        /// Data structure to hold all generated resource names for testing
        /// </summary>
        private class TestResourceNames
        {
            public string ResourceGroupName { get; }
            public string StorageAccountName { get; }
            public string ContainerRegistryName { get; }
            public string KeyVaultName { get; }
            public string SqlServerName { get; }
            public string SqlDatabaseName { get; }
            public string AppConfigurationName { get; }
            public string ContainerAppName { get; }
            public string ContainerAppsEnvironmentName { get; }

            public TestResourceNames(string projectName, string environmentName, string environmentShort, string uniqueSuffix)
            {
                ResourceGroupName = AzureResourceNaming.GenerateResourceGroupName(projectName, environmentName);
                StorageAccountName = AzureResourceNaming.GenerateStorageAccountName(projectName, environmentShort, uniqueSuffix);
                ContainerRegistryName = AzureResourceNaming.GenerateContainerRegistryName(projectName, environmentShort, uniqueSuffix);
                KeyVaultName = AzureResourceNaming.GenerateKeyVaultName(projectName, environmentShort, uniqueSuffix);
                SqlServerName = AzureResourceNaming.GenerateSqlServerName(projectName, environmentShort, uniqueSuffix);
                SqlDatabaseName = AzureResourceNaming.GenerateSqlDatabaseName(projectName, environmentShort);
                AppConfigurationName = AzureResourceNaming.GenerateAppConfigurationName(projectName, environmentShort, uniqueSuffix);
                ContainerAppName = AzureResourceNaming.GenerateContainerAppName(projectName, environmentShort);
                ContainerAppsEnvironmentName = AzureResourceNaming.GenerateContainerAppsEnvironmentName(projectName, environmentShort);
            }

            public string[] GetAllResourceNames()
            {
                return new[]
                {
                    ResourceGroupName,
                    StorageAccountName,
                    ContainerRegistryName,
                    KeyVaultName,
                    SqlServerName,
                    SqlDatabaseName,
                    AppConfigurationName,
                    ContainerAppName,
                    ContainerAppsEnvironmentName
                };
            }

            public string[] GetKebabCaseResourceNames()
            {
                return new[]
                {
                    ResourceGroupName,
                    KeyVaultName,
                    SqlServerName,
                    SqlDatabaseName,
                    AppConfigurationName,
                    ContainerAppName,
                    ContainerAppsEnvironmentName
                }.Where(name => name.Contains("-")).ToArray();
            }


        }

        [Fact]
        public void ResourceGroup_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var resourceGroupName = _resourceNames.ResourceGroupName;

            // Assert - Azure Resource Group naming rules
            resourceGroupName.Length.Should().BeInRange(1, 90, "Resource group names must be 1-90 characters");
            resourceGroupName.Should().MatchRegex(@"^[a-zA-Z0-9\-_\.\(\)]+$", "Resource group names can contain alphanumerics, underscores, hyphens, periods, and parentheses");
            resourceGroupName.Should().NotEndWith(".", "Resource group names cannot end with a period");
            resourceGroupName.Should().NotContain(" ", "Resource group names should not contain spaces for consistency");
        }

        [Fact]
        public void StorageAccount_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var storageAccountName = _resourceNames.StorageAccountName;

            // Assert - Azure Storage Account naming rules
            storageAccountName.Length.Should().BeInRange(3, 24, "Storage account names must be 3-24 characters");
            storageAccountName.Should().MatchRegex(@"^[a-z0-9]+$", "Storage account names can only contain lowercase letters and numbers");
            storageAccountName.Should().NotContain("-", "Storage account names cannot contain hyphens");
            storageAccountName.Should().NotContain("_", "Storage account names cannot contain underscores");
        }

        [Fact]
        public void ContainerRegistry_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var containerRegistryName = _resourceNames.ContainerRegistryName;

            // Assert - Azure Container Registry naming rules
            containerRegistryName.Length.Should().BeInRange(5, 50, "Container registry names must be 5-50 characters");
            containerRegistryName.Should().MatchRegex(@"^[a-zA-Z0-9]+$", "Container registry names can only contain alphanumerics");
            containerRegistryName.Should().NotContain("-", "Container registry names cannot contain hyphens");
            containerRegistryName.Should().NotContain("_", "Container registry names cannot contain underscores");
        }

        [Fact]
        public void KeyVault_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var keyVaultName = _resourceNames.KeyVaultName;

            // Assert - Azure Key Vault naming rules
            keyVaultName.Length.Should().BeInRange(3, 24, "Key vault names must be 3-24 characters");
            keyVaultName.Should().MatchRegex(@"^[a-zA-Z0-9\-]+$", "Key vault names can contain alphanumerics and hyphens");
            keyVaultName.Should().MatchRegex(@"^[a-zA-Z]", "Key vault names must start with a letter");
            keyVaultName.Should().MatchRegex(@"[a-zA-Z0-9]$", "Key vault names must end with a letter or number");
            keyVaultName.Should().NotMatchRegex(@"--", "Key vault names cannot contain consecutive hyphens");
        }

        [Fact]
        public void SqlServer_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var sqlServerName = _resourceNames.SqlServerName;

            // Assert - Azure SQL Server naming rules
            sqlServerName.Length.Should().BeInRange(1, 63, "SQL server names must be 1-63 characters");
            sqlServerName.Should().MatchRegex(@"^[a-z0-9\-]+$", "SQL server names can contain lowercase letters, numbers, and hyphens");
            sqlServerName.Should().NotStartWith("-", "SQL server names cannot start with hyphen");
            sqlServerName.Should().NotEndWith("-", "SQL server names cannot end with hyphen");
        }

        [Fact]
        public void SqlDatabase_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var sqlDatabaseName = _resourceNames.SqlDatabaseName;

            // Assert - Azure SQL Database naming rules
            sqlDatabaseName.Length.Should().BeInRange(1, 128, "SQL database names must be 1-128 characters");
            sqlDatabaseName.Should().NotMatchRegex(@"[<>*%&:\\/?]", "SQL database names cannot contain <>*%&:\\/?");
            sqlDatabaseName.Should().NotEndWith(".", "SQL database names cannot end with period");
            sqlDatabaseName.Should().NotEndWith(" ", "SQL database names cannot end with space");
        }

        [Fact]
        public void AppConfiguration_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var appConfigName = _resourceNames.AppConfigurationName;

            // Assert - Azure App Configuration naming rules
            appConfigName.Length.Should().BeInRange(5, 50, "App Configuration names must be 5-50 characters");
            appConfigName.Should().MatchRegex(@"^[a-zA-Z0-9\-]+$", "App Configuration names can contain alphanumerics and hyphens");
            appConfigName.Should().NotMatchRegex(@"---", "App Configuration names cannot contain more than two consecutive hyphens");
            appConfigName.Should().NotStartWith("-", "App Configuration names cannot start with hyphen");
            appConfigName.Should().NotEndWith("-", "App Configuration names cannot end with hyphen");
        }

        [Fact]
        public void ContainerApp_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var containerAppName = _resourceNames.ContainerAppName;

            // Assert - Azure Container Apps naming rules
            containerAppName.Length.Should().BeInRange(2, 32, "Container app names must be 2-32 characters");
            containerAppName.Should().MatchRegex(@"^[a-z0-9\-]+$", "Container app names can contain lowercase letters, numbers, and hyphens");
            containerAppName.Should().MatchRegex(@"^[a-z]", "Container app names must start with a letter");
            containerAppName.Should().MatchRegex(@"[a-z0-9]$", "Container app names must end with alphanumeric");
        }

        [Fact]
        public void ContainerAppsEnvironment_ShouldMeetAzureNamingRequirements()
        {
            // Arrange
            var containerAppsEnvName = _resourceNames.ContainerAppsEnvironmentName;

            // Assert - Azure Container Apps Environment naming rules
            containerAppsEnvName.Length.Should().BeInRange(2, 32, "Container apps environment names must be 2-32 characters");
            containerAppsEnvName.Should().MatchRegex(@"^[a-z0-9\-]+$", "Container apps environment names can contain lowercase letters, numbers, and hyphens");
            containerAppsEnvName.Should().MatchRegex(@"^[a-z]", "Container apps environment names must start with a letter");
            containerAppsEnvName.Should().MatchRegex(@"[a-z0-9]$", "Container apps environment names must end with alphanumeric");
        }

        [Fact]
        public void AllResourceNames_ShouldNotContainInvalidCharacters()
        {
            // Arrange
            var allResourceNames = _resourceNames.GetAllResourceNames();

            // Assert - Common restrictions across Azure resources
            foreach (var resourceName in allResourceNames)
            {
                resourceName.Should().NotContain("#", $"Resource name '{resourceName}' cannot contain # as it interferes with URL parsing");
                resourceName.Should().NotContain("@", $"Resource name '{resourceName}' should not contain @ symbol");
                resourceName.Should().NotContain("$", $"Resource name '{resourceName}' should not contain $ symbol");
                resourceName.Should().NotContain("%", $"Resource name '{resourceName}' should not contain % symbol");
                resourceName.Should().NotContain("&", $"Resource name '{resourceName}' should not contain & symbol");
                resourceName.Should().NotContain("*", $"Resource name '{resourceName}' should not contain * symbol");
                resourceName.Should().NotContain("+", $"Resource name '{resourceName}' should not contain + symbol");
                resourceName.Should().NotContain("=", $"Resource name '{resourceName}' should not contain = symbol");
                resourceName.Should().NotContain("?", $"Resource name '{resourceName}' should not contain ? symbol");
                resourceName.Should().NotContain("/", $"Resource name '{resourceName}' should not contain / symbol");
                resourceName.Should().NotContain("\\", $"Resource name '{resourceName}' should not contain \\ symbol");
                resourceName.Should().NotContain("<", $"Resource name '{resourceName}' should not contain < symbol");
                resourceName.Should().NotContain(">", $"Resource name '{resourceName}' should not contain > symbol");
                resourceName.Should().NotContain("|", $"Resource name '{resourceName}' should not contain | symbol");
                resourceName.Should().NotContain("\"", $"Resource name '{resourceName}' should not contain \" symbol");
                resourceName.Should().NotContain("'", $"Resource name '{resourceName}' should not contain ' symbol");
            }
        }

        [Fact]
        public void ResourceNames_ShouldFollowKebabCaseConvention()
        {
            // Arrange
            var kebabCaseResourceNames = _resourceNames.GetKebabCaseResourceNames();

            // Assert - Resources that support hyphens should use kebab-case
            foreach (var resourceName in kebabCaseResourceNames)
            {
                resourceName.Should().MatchRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$", 
                    $"Resource name '{resourceName}' with hyphens should follow kebab-case convention (lowercase with hyphens)");
            }
        }

        [Fact]
        public void ResourceNames_ShouldIncludeEnvironmentIdentifier()
        {
            // Arrange
            var resourceNames = _resourceNames.GetAllResourceNames();

            // Assert
            foreach (var resourceName in resourceNames)
            {
                resourceName.Should().Contain(TestEnvironmentShort.ToLowerInvariant(), 
                    $"Resource name '{resourceName}' should contain environment identifier");
            }
        }

        [Fact]
        public void ResourceNames_ShouldIncludeProjectIdentifier()
        {
            // Arrange
            var projectIdentifier = TestProjectName.ToLowerInvariant();
            var resourceNames = _resourceNames.GetAllResourceNames();

            // Assert
            foreach (var resourceName in resourceNames)
            {
                resourceName.Should().Contain(projectIdentifier, 
                    $"Resource name '{resourceName}' should contain project identifier");
            }
        }
    }
} 