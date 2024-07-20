using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.LLM
{
    public class SemanticKernelQuizGenerator : IQuizGenerator
    {
        private readonly Kernel _kernel;

        public SemanticKernelQuizGenerator(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<List<Question>> GenerateQuizQuestionsAsync(string text, int numberOfQuestions)
        {
            var prompt = @"Generate {{$numberOfQuestions}} quiz questions based on the following text. 
                           Each question should be multiple-choice with 4 options. 
                           Format the output as a JSON array of objects, where each object has 
                           'question', 'options' (array of 4 strings), and 'correctAnswerIndex' (index of correct option) properties.

                           Text: {{$text}}

                           Questions:";

            var function = _kernel.CreateFunctionFromPrompt(prompt);

            var result = await function.InvokeAsync(_kernel, new KernelArguments
            {
                ["text"] = text,
                ["numberOfQuestions"] = numberOfQuestions.ToString()
            });

            var jsonResult = result.GetValue<string>() ?? "[]";

            return ExtractQuestionsFromResult(jsonResult);
        }

        private List<Question> ExtractQuestionsFromResult(string result)
        {
            // Remove code fences, extra brackets, and whitespace using regex
            var cleanedResult = CleanJsonString(result);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };                

                var questions = JsonSerializer.Deserialize<List<Question>>(cleanedResult, options);

                return questions ?? new List<Question>();
            }
            catch (JsonException ex)
            {
                // Log the exception or handle it as appropriate for your application
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
                Console.WriteLine($"Cleaned JSON string: {cleanedResult}");
                return new List<Question>();
            }
        }

        private string CleanJsonString(string input)
        {
            // Remove code fences
            var withoutCodeFences = Regex.Replace(input, @"^\s*```(?:json)?\s*|\s*```\s*$", "", RegexOptions.Multiline);

            // Remove extra brackets at the start and end
            var withoutExtraBrackets = Regex.Replace(withoutCodeFences, @"^\s*\[?\s*|\s*\]?\s*$", "");

            // Trim whitespace and ensure the string is wrapped in brackets
            var trimmed = withoutExtraBrackets.Trim();
            if (!trimmed.StartsWith("["))
                trimmed = "[" + trimmed;
            if (!trimmed.EndsWith("]"))
                trimmed = trimmed + "]";

            return trimmed;
        }
    }
}