using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.DTOs;

public class QuizResultDto
{
    public int QuizId { get; set; }
    public List<QuestionResultDto> Questions { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
}