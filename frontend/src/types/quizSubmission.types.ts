import { Quiz } from './quiz.types';
import { ResultOption } from './quizResult.types';

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
  pointsEarned: number;
  submissionTime: Date;
}

export interface SubmissionDetail {
  quiz: Quiz;
  answers: ResultOption[];
  score: number;
  submissionTime: Date;
  userId?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
