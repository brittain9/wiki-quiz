using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class PromptManager
{
    private readonly string _directoryPath;
    private readonly Kernel _kernel;
    private readonly Dictionary<string, KernelFunction> _promptFunctions;
    private readonly IPromptTemplateFactory _promptTemplateFactory;

    public PromptManager(string directoryPath, IPromptTemplateFactory promptTemplateFactory = null)
    {
        _directoryPath = directoryPath;
        _kernel = new Kernel();
        _promptFunctions = new Dictionary<string, KernelFunction>();
        _promptTemplateFactory = promptTemplateFactory ?? new KernelPromptTemplateFactory();
    }

    private (string config, string prompt) LoadPromptTemplate(string templateName)
    {
        var templateDir = Path.Combine(_directoryPath, templateName);
        var configPath = Path.Combine(templateDir, "config.json");
        var promptPath = Path.Combine(templateDir, "skprompt.txt");

        if (!File.Exists(configPath) || !File.Exists(promptPath))
        {
            throw new ArgumentException($"Template '{templateName}' not found or is missing required files.");
        }
        var config = File.ReadAllText(configPath);
        var prompt = File.ReadAllText(promptPath);

        return (config, prompt);
    }

    private void CreatePromptFunction(string templateName)
    {
        var (config, prompt) = LoadPromptTemplate(templateName);
        PromptTemplateConfig skconfig = PromptTemplateConfig.FromJson(config);

        skconfig.Template = prompt;

        var promptFunction = _kernel.CreateFunctionFromPrompt(skconfig, _promptTemplateFactory);

        _promptFunctions[templateName] = promptFunction;
    }

    public KernelFunction GetPromptFunction(string templateName)
    {
        if (!_promptFunctions.ContainsKey(templateName))
        {
            CreatePromptFunction(templateName);
        }

        return _promptFunctions[templateName];
    }
}