using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.DTOs;

public class QuizSubmissionDto
{
    public int QuizId { get; set; }
    public List<int> UserAnswers { get; set; } = new List<int>();
}
