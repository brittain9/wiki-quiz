using System.Diagnostics;
using System.Text.Json;
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

            var jsonResult = result.GetValue<string>()?.Trim() ?? "[]";

            // TODO: Add error checking
            return JsonSerializer.Deserialize<List<Question>>(jsonResult) ?? new List<Question>();
        }

        public async Task<string> TestQuery(string text)
        {   
            var prompt = text;

            var function = _kernel.CreateFunctionFromPrompt(prompt);

            var result = await function.InvokeAsync(_kernel);

            return result.GetValue<string>()?.Trim() ?? "[]";
        }
    }
}