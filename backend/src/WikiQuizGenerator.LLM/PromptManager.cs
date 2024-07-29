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
    private readonly string _fallbackPromptFunctionName;

    public PromptManager(string directoryPath, IPromptTemplateFactory promptTemplateFactory = null)
    {
        _directoryPath = directoryPath;
        _kernel = new Kernel();
        _promptFunctions = new Dictionary<string, KernelFunction>();
        _promptTemplateFactory = promptTemplateFactory ?? new KernelPromptTemplateFactory();

        // Create our hardcoded fallback prompt function
        _fallbackPromptFunctionName = "Default";
        CreateFallbackPromptFunction();
    }

    public KernelFunction GetPromptFunction(string templateName, string? language)
    {
        var name = string.IsNullOrEmpty(language) ? 
            templateName : $"{templateName}_{language}";

        try
        {
            if (!_promptFunctions.ContainsKey(name))
            {
                // This will throw error if it fails
                CreatePromptFunction(templateName, language);
                Console.WriteLine($"Registered function for {name}");
            }
            return _promptFunctions[name];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return _promptFunctions[_fallbackPromptFunctionName];
        }
    }

    private void CreatePromptFunction(string templateName, string? language)
    {
        var name = string.IsNullOrEmpty(language) ?
            templateName : $"{templateName}_{language}";

        // This will handle any error that could arise during the creation of the function and result to using the fallback function
        try
        {
            // so we can load language specific or non-language specific prompt templates
            string templateDir = string.IsNullOrEmpty(language)
                ? Path.Combine(_directoryPath, templateName)
                : Path.Combine(_directoryPath, language, templateName);

            var configPath = Path.Combine(templateDir, "config.json");
            var promptPath = Path.Combine(templateDir, "skprompt.txt");

            if (!File.Exists(configPath) || !File.Exists(promptPath))
                throw new Exception($"Files not found.");
            

            var config = File.ReadAllText(configPath, System.Text.Encoding.UTF8);
            var prompt = File.ReadAllText(promptPath, System.Text.Encoding.UTF8);

            PromptTemplateConfig skconfig = PromptTemplateConfig.FromJson(config);
            skconfig.Template = prompt;

            var promptFunction = _kernel.CreateFunctionFromPrompt(skconfig, _promptTemplateFactory);

            _promptFunctions[name] = promptFunction;
        }
        catch (Exception ex)
        {
            throw new Exception($"$Error creating prompt function for {name}: {ex.Message}");
        }
    }

    private void CreateFallbackPromptFunction()
    {
        // The default prompt will be hardcoded so it can never fail to be loaded which would cause the app to crash in the current implementation.
        string defaultConfig = "{\r\n    \"name\": \"Default\",\r\n    \"description\": \"The default prompt when all else fails.\",\r\n    \"template_format\": \"semantic-kernel\",\r\n    \"input_variables\": [\r\n      {\r\n        \"name\": \"text\",\r\n        \"description\": \"The content based on which the quiz questions will be created.\"\r\n      },\r\n      {\r\n        \"name\": \"numQuestions\",\r\n        \"description\": \"The number of multiple-choice questions to create.\"\r\n      },\r\n      {\r\n        \"name\": \"numOptions\",\r\n        \"description\": \"The number of option choices for each question.\"\r\n      },\r\n      {\r\n        \"name\": \"language\",\r\n        \"description\": \"The language in which the quiz questions and options should be generated.\"\r\n      }\r\n    ]\r\n  }";
        string defaultPrompt = @"
        You are an expert quiz creator. Your task is to create an engaging and informative quiz based on the given content.

        Content: {{$text}}

        Number of questions: {{$numQuestions}}

        Number of option choices: {{$numOptions}}

        Language: {{$language}}

        Instructions:
        1. Create {numQuestions} multiple-choice questions based on the provided content.
        2. Each question should be independent and not require knowledge from other questions.
        3. Focus on key concepts, interesting facts, and important ideas from the content.
        4. Avoid questions about specific dates or names of people who are not well-known.
        5. For each question, provide {{$numOptions}} options, with only one correct answer.
        6. Output each question in an array in JSON format, following this structure:

            {
              ""Text"": ""Question text in {{$language}}"",
              ""Options"": [
                ""Option 1 in {{$language}}"",
                ""Option 2 in {{$language}}"",
                ""Option 3 in {{$language}}"",
                ""Option 4 in {{$language}}""
              ],
              ""CorrectAnswerIndex"": number
            }

        7. Keep the JSON keys (""Text"", ""Options"", ""CorrectAnswerIndex"") in English.

        Most importantly, return ONLY valid JSON.
        Generate the quiz questions in {{$language}} while maintaining the JSON structure as specified:
        ";

        PromptTemplateConfig skconfig = PromptTemplateConfig.FromJson(defaultConfig);
        skconfig.Template = defaultPrompt;

        var promptFunction = _kernel.CreateFunctionFromPrompt(skconfig, _promptTemplateFactory);

        _promptFunctions[_fallbackPromptFunctionName] = promptFunction;
    }
}
