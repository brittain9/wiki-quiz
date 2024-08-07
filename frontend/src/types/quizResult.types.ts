export interface QuizResult {
    id: number;
    title: string;
    createdAt: string;
    aiResponses: AIResponseResult[];
    totalQuestions: number;
    correctAnswers: number;
  }
  
  export interface AIResponseResult {
    responseTopic: string;
    questions: QuestionResult[];
  }
  
  export interface QuestionResult {
    id: number;
    text: string;
    options: string[];
    correctOptionNumber: number;
    userSelectedOption: number | null;
  }