import { apiGet, parseApiError } from '../apiService';

import type { SubmissionResponse, SubmissionDetail } from '../../types';

// Submission API endpoints
const SUBMISSION_ENDPOINTS = {
  QUIZ_SUBMISSION: (id: number) => `/submission/quizsubmission/${id}`,
  RECENT: '/submission/quizsubmission/recent',
  MY_SUBMISSIONS: '/submission/my-submissions',
} as const;

/**
 * Submission service for handling all submission-related API requests
 */
export const submissionApi = {
  /**
   * Get a user's quiz submission by ID
   */
  async getQuizSubmissionById(id: number): Promise<SubmissionDetail> {
    try {
      return await apiGet<SubmissionDetail>(
        SUBMISSION_ENDPOINTS.QUIZ_SUBMISSION(id),
      );
    } catch (error) {
      console.error(
        `Failed to get quiz submission by ID: ${parseApiError(error)}`,
      );
      throw error;
    }
  },

  /**
   * Get the 10 most recent submissions for the current user
   * Always returns an array, even if the API returns null or undefined
   */
  async getRecentSubmissions(): Promise<SubmissionResponse[]> {
    try {
      const response = await apiGet<SubmissionResponse[]>(
        SUBMISSION_ENDPOINTS.RECENT,
      );
      // Ensure we always return an array
      return Array.isArray(response) ? response : [];
    } catch (error) {
      console.error(
        `Failed to get recent submissions: ${parseApiError(error)}`,
      );
      // Return empty array on error instead of throwing
      return [];
    }
  },

  /**
   * Get all submissions for the current user
   */
  async getMySubmissions(): Promise<SubmissionResponse[]> {
    try {
      const response = await apiGet<SubmissionResponse[]>(
        SUBMISSION_ENDPOINTS.MY_SUBMISSIONS,
      );
      // Ensure we always return an array
      return Array.isArray(response) ? response : [];
    } catch (error) {
      console.error(`Failed to get user submissions: ${parseApiError(error)}`);
      // Return empty array on error instead of throwing
      return [];
    }
  },
};

export default submissionApi;
