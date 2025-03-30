import { Quiz } from './quiz.types';

export interface QuizSubmission {
  quizId: number;
  questionAnswers: QuestionAnswer[];
}

export interface QuestionAnswer {
  questionId: number;
  selectedOptionNumber: number;
}

export interface SubmissionResponse {
  id: number;
  title: string;
  score: number;
  submissionTime: Date;
}

export interface SubmissionDetail {
  quiz: Quiz;
  questionAnswers: QuestionAnswer[];
  score: number;
  submissionTime: Date;
}
