using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Mappers;

internal class QuestionMapper
{
    public static QuestionDto ToDto(Question question)
    {
        var options = new List<string>
        {
            question.Option1,
            question.Option2
        };

        if (!string.IsNullOrEmpty(question.Option3))
            options.Add(question.Option3);
        if (!string.IsNullOrEmpty(question.Option4))
            options.Add(question.Option4);
        if (!string.IsNullOrEmpty(question.Option5))
            options.Add(question.Option5);

        return new QuestionDto
        {
            Text = question.Text,
            Options = options
        };
    }
}