import api, { BasicQuizParams } from './api';
import { Quiz } from '../types/quiz.types';
import { QuizSubmission, QuestionAnswer } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

export const quizService = {
  async fetchQuiz(params: BasicQuizParams): Promise<Quiz> {
    try {
      return await api.getBasicQuiz(params);
    } catch (error) {
      console.error('Error fetching quiz:', error);
      throw error;
    }
  },

  async submitQuiz(quizId: number, userAnswers: QuestionAnswer[]): Promise<QuizResult> {
    try {
      const submission: QuizSubmission = {
        quizId,
        questionAnswers: userAnswers
      };
      return await api.submitQuiz(submission);
    } catch (error) {
      console.error('Error submitting quiz:', error);
      throw error;
    }
  }
};