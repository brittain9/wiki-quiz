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

        public async Task<List<QuizQuestion>> GenerateQuizQuestionsAsync(string text, int numberOfQuestions)
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

            return JsonSerializer.Deserialize<List<QuizQuestion>>(jsonResult) ?? new List<QuizQuestion>();
        }
    }
}