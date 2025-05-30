using Pulumi;
using Pulumi.AzureNative.Authorization;
using System.Collections.Generic;

namespace WikiQuiz.Infrastructure
{
    public class StackConfig
    {
        private readonly Config _config;
        private readonly Config _azureNativeConfig;

        public string Location { get; }
        public string ProjectName { get; }
        public string EnvironmentName { get; }
        public string ContainerImageName { get; }
        public string ContainerImageTag { get; }
        public int ContainerPort { get; }
        public string PostgresAdminLogin { get; }
        public Output<string> PostgresAdminPassword { get; } // Secret
        public string PostgresDatabaseName { get; }
        public Output<string> OpenAiApiKey { get; }           // Secret
        public Output<string> GoogleClientId { get; }         // Secret
        public Output<string> GoogleClientSecret { get; }     // Secret
        public string JwtIssuer { get; }
        public string JwtAudience { get; }
        public Output<string> JwtSecret { get; }              // Secret
        public string FrontendUrl { get; }
        
        // Custom Domain Configuration
        public string? CustomDomain { get; }
        public bool EnableCustomDomain { get; }
        public string? CertificateId { get; }

        public StackConfig()
        {
            _config = new Config();
            // Config for azure-native provider, typically for location
            _azureNativeConfig = new Config("azure-native"); 

            // Load configuration values
            // Defaults are based on your main.parameters.json and deploy.ps1
            Location = _azureNativeConfig.Get("location") ?? "centralus"; 
            ProjectName = _config.Require("projectName"); 
            EnvironmentName = _config.Get("environmentName") ?? "Development"; 
            ContainerImageName = _config.Get("containerImageName") ?? "wikiquiz-api"; 
            ContainerImageTag = _config.Get("containerImageTag") ?? "latest"; 
            ContainerPort = _config.GetInt32("containerPort") ?? 8080; 
            PostgresAdminLogin = _config.Require("postgresAdminLogin"); 
            PostgresAdminPassword = _config.RequireSecret("postgresAdminPassword"); 
            PostgresDatabaseName = _config.Get("postgresDatabaseName") ?? "WikiQuizGenerator"; 
            OpenAiApiKey = _config.RequireSecret("openAiApiKey");            
            GoogleClientId = _config.RequireSecret("googleClientId");        
            GoogleClientSecret = _config.RequireSecret("googleClientSecret"); 
            JwtIssuer = _config.Get("jwtIssuer") ?? GenerateDefaultJwtIssuer(); 
            JwtAudience = _config.Get("jwtAudience") ?? GenerateDefaultJwtAudience(); 
            JwtSecret = _config.RequireSecret("jwtSecret");                  
            FrontendUrl = _config.Require("frontendUrl");
            
            // Custom Domain Configuration
            CustomDomain = _config.Get("customDomain");
            EnableCustomDomain = _config.GetBoolean("enableCustomDomain") ?? false;
            CertificateId = _config.Get("certificateId");
        }
        
        private string GenerateDefaultJwtIssuer()
        {
            if (!string.IsNullOrEmpty(CustomDomain) && EnableCustomDomain)
            {
                return $"https://{CustomDomain}/";
            }
            
            return EnvironmentName.ToLower() switch
            {
                "production" => "https://api.quiz.alexanderbrittain.com/",
                "test" => "https://api-test.quiz.alexanderbrittain.com/",
                _ => "https://api-dev.quiz.alexanderbrittain.com/"
            };
        }
        
        private string GenerateDefaultJwtAudience()
        {
            return EnvironmentName.ToLower() switch
            {
                "production" => "wikiquiz-api-prod",
                "test" => "wikiquiz-api-test", 
                _ => "wikiquiz-api-dev"
            };
        }
    }
} 