using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikiQuizGenerator.Core.Models;

public class QuizSubmission
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; }
    public List<int> Answers { get; set; } = new List<int>();
    public DateTime SubmissionTime { get; set; }
    public int Score { get; set; }
}