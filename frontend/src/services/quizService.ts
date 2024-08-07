import api, { BasicQuizParams } from './api';
import { Quiz, AIResponse, Question } from '../types/quiz.types';
import { QuizSubmission, UserAnswer } from '../types/quizSubmission.types';
import { QuizResult } from '../types/quizResult.types';

export const quizService = {
  async fetchQuiz(params: BasicQuizParams): Promise<Quiz> {
    try {
      console.log('Fetching quiz with params:', params);
      const quiz = await api.getBasicQuiz(params);
      console.log('Fetched quiz:', quiz);
      return quiz;
    } catch (error) {
      console.error('Error in fetchQuiz:', error);
      throw error;
    }
  },

  async submitQuiz(quizId: number, userAnswers: UserAnswer[]): Promise<QuizResult> {
    try {
      const submission: QuizSubmission = { quizId, userAnswers };
      console.log('Submitting quiz:', submission);
      const result = await api.submitQuiz(submission);
      console.log('Quiz submission result:', result);
      return result;
    } catch (error) {
      console.error('Error in quizService.submitQuiz:', error);
      throw error;
    }
  }
};
