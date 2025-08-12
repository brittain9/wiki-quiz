import { apiPost, parseApiError } from '../apiService';

import type { Quiz, QuizSubmission, SubmissionResponse } from '../../types';

// Quiz API endpoints
const QUIZ_ENDPOINTS = {
  GENERATE: '/quiz/generate',
  SUBMIT_QUIZ: '/quiz/submit',
  VALIDATE_ANSWER: '/quiz/validate',
} as const;

/**
 * Quiz service for handling all quiz-related API requests
 */
export const quizApi = {
  /**
   * Generate a basic quiz from topic/language
   * @param params - Query parameters for quiz generation
   */
  async generateBasicQuiz(params: {
    topic: string;
    aiService?: string;
    model?: string;
    language?: string;
    numQuestions?: number;
    numOptions?: number;
    extractLength?: number;
  }): Promise<Quiz> {
    try {
      // Default values for optional params
      const {
        topic,
        aiService,
        model,
        language = 'en',
        numQuestions = 5,
        numOptions = 4,
        extractLength = 1000,
      } = params;
      // TODO: better
      const query =
        `?topic=${encodeURIComponent(topic)}` +
        (aiService ? `&aiService=${encodeURIComponent(aiService)}` : '') +
        (model ? `&model=${encodeURIComponent(model)}` : '') +
        `&language=${encodeURIComponent(language)}` +
        `&numQuestions=${numQuestions}` +
        `&numOptions=${numOptions}` +
        `&extractLength=${extractLength}`;
      return await apiPost<Quiz>(`${QUIZ_ENDPOINTS.GENERATE}${query}`);
    } catch (error) {
      console.error(`Failed to generate basic quiz: ${parseApiError(error)}`);
      throw error;
    }
  },

  /**
   * Submit quiz answers
   * @param submission - The quiz submission data
   */
  async submitQuiz(submission: QuizSubmission): Promise<SubmissionResponse> {
    try {
      return await apiPost<SubmissionResponse, QuizSubmission>(
        QUIZ_ENDPOINTS.SUBMIT_QUIZ,
        submission,
      );
    } catch (error) {
      console.error(`Failed to submit quiz: ${parseApiError(error)}`);
      throw error;
    }
  },

  /**
   * Validate a single question answer
   */
  async validateAnswer(params: {
    quizId: number;
    questionId: number;
    selectedOptionNumber: number;
  }): Promise<{
    isCorrect: boolean;
    correctOptionNumber: number;
    pointsEarned: number;
    correctAnswerText?: string;
  }> {
    try {
      return await apiPost(
        QUIZ_ENDPOINTS.VALIDATE_ANSWER,
        params,
      );
    } catch (error) {
      console.error(`Failed to validate answer: ${parseApiError(error)}`);
      throw error;
    }
  },
};

export default quizApi;
