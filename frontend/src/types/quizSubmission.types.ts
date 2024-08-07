export interface QuizSubmission {
    quizId: number;
    questionAnswers: QuestionAnswer[];
  }
  
  export interface QuestionAnswer {
    questionId: number;
    selectedOptionNumber: number;
  }