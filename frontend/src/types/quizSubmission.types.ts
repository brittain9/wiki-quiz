export interface QuizSubmission {
    quizId: number;
    userAnswers: UserAnswer[];
  }
  
  export interface UserAnswer {
    questionId: number;
    selectedOptionNumber: number;
  }