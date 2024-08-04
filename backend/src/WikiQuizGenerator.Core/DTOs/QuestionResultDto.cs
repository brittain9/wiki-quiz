using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.DTOs;

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public int UserAnswer { get; set; }
    public int CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}