using Pulumi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WikiQuiz.Infrastructure
{
    public class StackConfig
    {
        private readonly Config _config;
        private readonly Config _azureNativeConfig;

        // Core Configuration
        public string Location { get; }
        public string ProjectName { get; }
        public string EnvironmentName { get; }
        public string EnvironmentShort { get; }
        
        // Container Configuration
        public string ContainerImageName { get; }
        public string ContainerImageTag { get; }
        public int ContainerPort { get; }
        
        // Database Configuration
        public string PostgresAdminLogin { get; }
        public Output<string> PostgresAdminPassword { get; }
        public string PostgresDatabaseName { get; }
        
        // External Service Secrets
        public Output<string> OpenAiApiKey { get; }
        public Output<string> GoogleClientId { get; }
        public Output<string> GoogleClientSecret { get; }
        
        // JWT Configuration
        public string JwtIssuer { get; }
        public string JwtAudience { get; }
        public Output<string> JwtSecret { get; }
        
        // Frontend Configuration
        public string FrontendUrl { get; }
        

        // Environment-specific configurations
        public EnvironmentConfig EnvConfig { get; }

        public StackConfig()
        {
            _config = new Config("WikiQuizGenerator.Pulumi");
            _azureNativeConfig = new Config("azure-native");

            // Core configuration with validation
            Location = ValidateLocation(_azureNativeConfig.Get("location") ?? "centralus");
            ProjectName = ValidateProjectName(_config.Require("projectName"));
            EnvironmentName = ValidateEnvironmentName(_config.Get("environmentName") ?? "Development");
            EnvironmentShort = GetEnvironmentShort(EnvironmentName);
            
            // Container configuration
            ContainerImageName = _config.Get("containerImageName") ?? "wikiquizapi";
            ContainerImageTag = _config.Get("containerImageTag") ?? GetDefaultImageTag();
            ContainerPort = _config.GetInt32("containerPort") ?? 8080;
            
            // Database configuration
            PostgresAdminLogin = _config.Get("postgresAdminLogin") ?? "wikiquizadmin";
            PostgresAdminPassword = _config.GetSecret("postgresAdminPassword") ?? Output.Create("dummyPasswordForPreview");
            PostgresDatabaseName = _config.Get("postgresDatabaseName") ?? "WikiQuizGenerator";
            
            // External service secrets
            OpenAiApiKey = _config.GetSecret("openAiApiKey") ?? Output.Create("dummyApiKeyForPreview");
            GoogleClientId = _config.GetSecret("googleClientId") ?? Output.Create("dummyClientIdForPreview");
            GoogleClientSecret = _config.GetSecret("googleClientSecret") ?? Output.Create("dummyClientSecretForPreview");
            JwtSecret = _config.GetSecret("jwtSecret") ?? Output.Create("dummyJwtSecretForPreview");
            
            
            // JWT and Frontend configuration (derived from environment and custom domain)
            JwtIssuer = _config.Get("jwtIssuer") ?? GenerateDefaultJwtIssuer();
            JwtAudience = _config.Get("jwtAudience") ?? GenerateDefaultJwtAudience();
            FrontendUrl = _config.Get("frontendUrl") ?? GenerateDefaultFrontendUrl();
            
            // Environment-specific configuration
            EnvConfig = new EnvironmentConfig(EnvironmentName);
        }

        private string ValidateLocation(string location)
        {
            var validLocations = new HashSet<string>
            {
                "centralus", "eastus", "eastus2", "westus", "westus2", "westus3",
                "northcentralus", "southcentralus", "westcentralus",
                "northeurope", "westeurope", "uksouth", "ukwest",
                "eastasia", "southeastasia", "japaneast", "japanwest",
                "australiaeast", "australiasoutheast", "brazilsouth",
                "canadacentral", "canadaeast", "francecentral", "germanywestcentral",
                "koreacentral", "norwayeast", "southafricanorth", "switzerlandnorth",
                "uaenorth", "centralindia", "southindia", "westindia"
            };
            
            if (!validLocations.Contains(location.ToLower()))
            {
                throw new ArgumentException($"Invalid Azure location: {location}");
            }
            
            return location.ToLower();
        }

        private string ValidateProjectName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName) || projectName.Length > 20)
            {
                throw new ArgumentException("Project name must be 1-20 characters");
            }
            
            if (!System.Text.RegularExpressions.Regex.IsMatch(projectName, "^[a-zA-Z][a-zA-Z0-9]*$"))
            {
                throw new ArgumentException("Project name must start with a letter and contain only alphanumeric characters");
            }
            
            return projectName.ToLower();
        }

        private string ValidateEnvironmentName(string environmentName)
        {
            var validEnvironments = new HashSet<string> { "Development", "Production" };
            
            if (!validEnvironments.Contains(environmentName))
            {
                throw new ArgumentException($"Environment must be one of: {string.Join(", ", validEnvironments)}");
            }
            
            return environmentName;
        }

        private string GetEnvironmentShort(string environmentName) => environmentName switch
        {
            "Production" => "prd",
            "Development" => "dev",
            _ => throw new ArgumentException($"Unknown environment: {environmentName}")
        };

        private string GetDefaultImageTag() => EnvironmentName switch
        {
            "Production" => "stable",
            "Development" => "dev",
            _ => "latest"
        };

        private string GenerateDefaultJwtIssuer()
        {
            return EnvironmentName switch
            {
                "Production" => "https://api.quiz.alexanderbrittain.com/",
                "Development" => "https://api-dev.quiz.alexanderbrittain.com/",
                _ => throw new ArgumentException($"Unknown environment: {EnvironmentName}")
            };
        }
        
        private string GenerateDefaultJwtAudience() => $"wikiquiz-api-{EnvironmentShort}";

        private string GenerateDefaultFrontendUrl() => EnvironmentName switch
        {
            "Production" => "https://quiz.alexanderbrittain.com",
            "Development" => "https://localhost:3000",
            _ => throw new ArgumentException($"Unknown environment: {EnvironmentName}")
        };
    }

    public class EnvironmentConfig
    {
        public string Environment { get; }
        public DatabaseConfig Database { get; }
        public ContainerConfig Container { get; }
        public ScalingConfig Scaling { get; }
        public bool HighAvailabilityEnabled { get; }

        public EnvironmentConfig(string environment)
        {
            Environment = environment;
            
            (Database, Container, Scaling, HighAvailabilityEnabled) = environment switch
            {
                "Production" => (
                    new DatabaseConfig("Standard_B2s", "Burstable", 32, 7, false),
                    new ContainerConfig(0.5, 1.0),
                    new ScalingConfig(1, 3),
                    false
                ),
                "Development" => (
                    new DatabaseConfig("Standard_B1ms", "Burstable", 32, 7, false),
                    new ContainerConfig(0.25, 0.5),
                    new ScalingConfig(0, 1),
                    false
                ),
                _ => throw new ArgumentException($"Unknown environment: {environment}")
            };
        }
    }

    public record DatabaseConfig(
        string SkuName,
        string SkuTier,
        int StorageSizeGB,
        int BackupRetentionDays,
        bool GeoRedundantBackup
    );

    public record ContainerConfig(
        double CpuCores,
        double MemoryGB
    );

    public record ScalingConfig(
        int MinReplicas,
        int MaxReplicas
    );
}
