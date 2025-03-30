import { Quiz } from './quiz.types';

export interface QuizResult {
  quiz: Quiz;
  answers: ResultOption[];
  submissionTime: Date;
  score: number;
}

export interface ResultOption {
  questionId: number;
  correctAnswerChoice: number;
  selectedAnswerChoice: number;
}
