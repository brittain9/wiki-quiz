using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                // Get the time because the user currently has to wait for the function be created before they can get a quiz.
                Stopwatch timer = new Stopwatch();
                timer.Start();
                CreatePromptFunction(templateName, language); // This will throw error if it fails
                timer.Stop();
                Console.WriteLine($"Registered function for {name} in {timer.ElapsedMilliseconds} milliseconds");
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
        string defaultConfig = @"{
            ""name"": ""Default"",
            ""description"": ""The default prompt when all else fails."",
            ""template_format"": ""semantic-kernel"",
            ""input_variables"": [
              {
                ""name"": ""text"",
                ""description"": ""The content based on which the quiz questions will be created.""
              },
              {
                ""name"": ""numQuestions"",
                ""description"": ""The number of multiple-choice questions to create.""
              },
              {
                ""name"": ""numOptions"",
                ""description"": ""The number of option choices for each question.""
              },
              {
                ""name"": ""language"",
                ""description"": ""The language in which the quiz questions and options should be generated.""
              }
            ]
          }";


        string defaultPrompt = @"
        You are an expert quiz creator. Your task is to create an engaging and informative quiz based on the given content.

        Content: {{$text}}

        Number of questions: {{$numQuestions}}

        Number of option choices: {{$numOptions}}

        Language: {{$language}}

        Instructions:
        1. Create {numQuestions} multiple-choice questions based on the provided content.
        2. Each question MUST have EXACTLY {{$numOptions}} multiple-choice options. No more, no less.
        3. Each question should be independent and not require knowledge from other questions.
        4. Focus on key concepts, interesting facts, and important ideas from the content.
        5. Avoid questions about specific dates or names of people who are not well-known.
        6. For each question, provide {{$numOptions}} options, with only one correct answer.
        7. The ""CorrectAnswerIndex"" MUST be the index of the correct answer in the Options array (0-based index).
        8. Output each question in an array in JSON format, following this structure:

            {
              ""Text"": ""Question text in {{$language}}"",
              ""Options"": [
                ""Option 1 in {{$language}}"",
                ""Option 2 in {{$language}}"",
                ...
              ],
              ""CorrectAnswerIndex"": number
            }

        9. Keep the JSON keys (""Text"", ""Options"", ""CorrectAnswerOption"") in English.

        Most importantly, return ONLY valid JSON.
        Generate the quiz questions in {{$language}} while maintaining the JSON structure as specified:
        ";

        PromptTemplateConfig skconfig = PromptTemplateConfig.FromJson(defaultConfig);
        skconfig.Template = defaultPrompt;

        var promptFunction = _kernel.CreateFunctionFromPrompt(skconfig, _promptTemplateFactory);

        _promptFunctions[_fallbackPromptFunctionName] = promptFunction;
    }
}
