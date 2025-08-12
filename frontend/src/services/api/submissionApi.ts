import { apiGet, apiDelete, parseApiError } from '../apiService';

import type { SubmissionResponse, SubmissionDetail, PaginatedResponse } from '../../types';

// User API endpoints (renamed from Submission)
const USER_ENDPOINTS = {
  QUIZ_SUBMISSION: (quizId: number) => `/user/submissions/${quizId}`,
  MY_SUBMISSIONS: '/user/submissions',
  USAGE: '/user/usage',
  STATS: '/user/stats',
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
      return await apiGet<SubmissionDetail>(USER_ENDPOINTS.QUIZ_SUBMISSION(id));
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
  // recent submissions endpoint removed in new design; can be re-added if needed

  /**
   * Get all submissions for the current user
   */
  async getMySubmissions(): Promise<SubmissionResponse[]> {
    try {
      // Backend returns a paginated response, not a plain array. Fetch first page.
      const response = await apiGet<PaginatedResponse<SubmissionResponse>>(USER_ENDPOINTS.MY_SUBMISSIONS + `?page=1&pageSize=10`);
      return response.items ?? [];
    } catch (error) {
      console.error(`Failed to get user submissions: ${parseApiError(error)}`);
      // Return empty array on error instead of throwing
      return [];
    }
  },

  /**
   * Get paginated submissions for the current user
   */
  async getMySubmissionsPaginated(
    page: number = 1,
    pageSize: number = 10,
  ): Promise<PaginatedResponse<SubmissionResponse>> {
    try {
      const url = `${USER_ENDPOINTS.MY_SUBMISSIONS}?page=${page}&pageSize=${pageSize}`;
      const response = await apiGet<PaginatedResponse<SubmissionResponse>>(url);
      return response;
    } catch (error) {
      console.error(
        `Failed to get paginated user submissions: ${parseApiError(error)}`,
      );
      // Return empty paginated response on error
      return {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: pageSize,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false,
      };
    }
  },

  async getUsage(): Promise<{ userId: string; isPremium: boolean; currentCost: number; weeklyCostLimit: number; remaining: number; periodDays: number; }> {
    try {
      return await apiGet(USER_ENDPOINTS.USAGE);
    } catch (error) {
      console.error(`Failed to get usage: ${parseApiError(error)}`);
      throw error;
    }
  },

  async getStats(): Promise<{ userId: string; totalPoints: number; level: number; nextLevel: number; pointsRequiredForNextLevel: number; pointsToNextLevel: number; }> {
    try {
      return await apiGet(USER_ENDPOINTS.STATS);
    } catch (error) {
      console.error(`Failed to get stats: ${parseApiError(error)}`);
      throw error;
    }
  },

  /**
   * Clear all submitted quizzes for current user
   */
  async clearUserSubmissions(): Promise<{ deleted: number }> {
    try {
      return await apiDelete<{ deleted: number }>(USER_ENDPOINTS.MY_SUBMISSIONS);
    } catch (error) {
      console.error(`Failed to clear submissions: ${parseApiError(error)}`);
      throw error;
    }
  },
};

export default submissionApi;
