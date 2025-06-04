using Pulumi;
using System.Collections.Generic;

namespace WikiQuiz.Infrastructure.Modules
{
    // This module is a placeholder for custom domain functionality
    // Custom domain support has been removed as per requirements
    
    public class CustomDomainModuleArgs
    {
        // Minimal args to maintain compatibility
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }

    public class CustomDomainModule : ComponentResource
    {
        public CustomDomainModule(string name, CustomDomainModuleArgs args, ComponentResourceOptions? options = null)
            : base("wikiquiz:modules:CustomDomainModule", name, options)
        {
            // This module is now a no-op since custom domain functionality has been removed
            this.RegisterOutputs();
        }
    }
}
