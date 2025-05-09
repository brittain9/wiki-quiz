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
        public string ContainerImage { get; }
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
            ContainerImage = _config.Require("containerImage"); 
            ContainerPort = _config.GetInt32("containerPort") ?? 8080; 
            PostgresAdminLogin = _config.Require("postgresAdminLogin"); 
            PostgresAdminPassword = _config.RequireSecret("postgresAdminPassword"); 
            PostgresDatabaseName = _config.Get("postgresDatabaseName") ?? "WikiQuizGenerator"; 
            OpenAiApiKey = _config.RequireSecret("openAiApiKey");            
            GoogleClientId = _config.RequireSecret("googleClientId");        
            GoogleClientSecret = _config.RequireSecret("googleClientSecret"); 
            JwtIssuer = _config.Require("jwtIssuer"); 
            JwtAudience = _config.Require("jwtAudience"); 
            JwtSecret = _config.RequireSecret("jwtSecret");                  
            FrontendUrl = _config.Require("frontendUrl"); 
        }
    }
} 