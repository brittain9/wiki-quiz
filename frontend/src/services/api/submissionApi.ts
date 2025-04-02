// submissionApi.ts
import axios from 'axios';

import { QuizResult } from '../../types/quizResult.types';
import {
  QuizSubmission,
  SubmissionResponse,
} from '../../types/quizSubmission.types';
import apiClient from '../apiClient';
import { parseApiError } from './utils';

export const submissionApi = {
  /**
   * Submits the answers for a specific quiz.
   */
  submitQuiz: async (
    submission: QuizSubmission,
  ): Promise<SubmissionResponse> => {
    try {
      const response = await apiClient.post<SubmissionResponse>(
        '/quiz/submitquiz',
        submission, // Send submission data in body
      );
      return response.data;
    } catch (error) {
      console.error('Error submitting quiz:', error);
      throw new Error(`Failed to submit quiz: ${parseApiError(error)}`);
    }
  },

  /**
   * Retrieves a list of recent quiz submissions for the current user.
   */
  getRecentSubmissions: async (): Promise<SubmissionResponse[]> => {
    try {
      const response = await apiClient.get<SubmissionResponse[]>(
        '/submission/quizsubmission/recent',
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching recent submissions:', error);
      throw new Error(
        `Failed to fetch recent submissions: ${parseApiError(error)}`,
      );
    }
  },

  /**
   * Retrieves the details of a specific quiz submission by its ID.
   */
  getSubmissionById: async (id: number): Promise<QuizResult> => {
    try {
      const response = await apiClient.get<QuizResult>(
        `/submission/quizsubmission/${id}`,
      );
      return response.data;
    } catch (error) {
      // Keep specific 404 handling here if desired
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        throw new Error(`Submission with ID ${id} not found.`);
      }
      console.error(`Error fetching submission with ID ${id}:`, error);
      throw new Error(
        `Failed to fetch submission ${id}: ${parseApiError(error)}`,
      );
    }
  },
};
