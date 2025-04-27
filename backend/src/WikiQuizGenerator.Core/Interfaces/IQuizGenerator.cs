using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizGenerator
{
    Task<Quiz> GenerateBasicQuizAsync(string topic, Languages language, string aiService, string model, int numQuestions, int numOptions, int extractLength);
}

