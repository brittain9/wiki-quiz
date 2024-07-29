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

    public KernelFunction GetPromptFunction(string templateName, string language = "en")
    {
        var nameWithLang = templateName + "_" + language;

        if (!_promptFunctions.ContainsKey(nameWithLang))
            CreatePromptFunction(templateName, language);

        return _promptFunctions[nameWithLang];
    }

    private void CreatePromptFunction(string templateName, string language)
    {
        var (config, prompt) = LoadPromptTemplate(templateName, language);

        PromptTemplateConfig skconfig = PromptTemplateConfig.FromJson(config);
        skconfig.Template = prompt;

        var promptFunction = _kernel.CreateFunctionFromPrompt(skconfig, _promptTemplateFactory);

        var nameWithLang = templateName + "_" + language;
        _promptFunctions[nameWithLang] = promptFunction;
        Console.WriteLine($"Registered function for {nameWithLang}");
    }

    private (string config, string prompt) LoadPromptTemplate(string templateName, string language)
    {
        var templateDir = Path.Combine(_directoryPath, language, templateName);
        var configPath = Path.Combine(templateDir, "config.json");
        var promptPath = Path.Combine(templateDir, "skprompt.txt");

        if (!File.Exists(configPath) || !File.Exists(promptPath))
        {
            throw new ArgumentException($"Template '{templateName}' not found or is missing required files.");
        }  

        var config = File.ReadAllText(configPath, System.Text.Encoding.UTF8);
        var prompt = File.ReadAllText(promptPath, System.Text.Encoding.UTF8);

        Console.WriteLine($"Loaded config for {language}: {config}");
        Console.WriteLine($"Loaded prompt for {language}: {prompt}");

        return (config, prompt);
    }
}