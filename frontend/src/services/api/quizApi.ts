// quizApi.ts
import axios from 'axios';

import { Quiz } from '../../types/quiz.types';
import apiClient from '../apiClient';
import { parseApiError } from './utils';

// Interface for parameters needed to create a basic quiz
export interface CreateBasicQuizRequest {
  topic: string;
  aiService: string | null;
  model: string | null;
  language?: string;
  numQuestions?: number;
  numOptions?: number;
  extractLength?: number;
}

export const quizApi = {
  /**
   * Creates a new basic quiz based on the provided options.
   * Uses POST with query parameters, not body
   */
  createBasicQuiz: async (
    quizOptions: CreateBasicQuizRequest,
  ): Promise<Quiz> => {
    try {
      // Use POST with query parameters, not body
      const response = await apiClient.post<Quiz>(
        '/quiz/basicquiz',
        {}, // Empty body
        {
          params: quizOptions, // Send as query parameters
        },
      );
      return response.data;
    } catch (error) {
      // Specific handling for 404 from this endpoint based on original code's intent
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        throw new Error(`Please enter a valid Wikipedia topic.`);
      }
      console.error('Error creating basic quiz:', error);
      throw new Error(`${parseApiError(error)}`);
    }
  },

  /**
   * Retrieves the available AI service options.
   */
  getAiServices: async (): Promise<string[]> => {
    try {
      const response = await apiClient.get<string[]>('/ai/services');
      return response.data;
    } catch (error) {
      console.error('Error fetching AI services:', error);
      throw new Error(`Failed to fetch AI services: ${parseApiError(error)}`);
    }
  },

  /**
   * Retrieves the available AI models for a specific service ID.
   */
  getAiModels: async (serviceId: string): Promise<string[]> => {
    try {
      const response = await apiClient.get<string[]>(
        '/ai/models',
        {
          params: { aiServiceId: serviceId },
        },
      );
      return response.data;
    } catch (error) {
      console.error(
        `Error fetching AI models for service ${serviceId}:`,
        error,
      );
      throw new Error(
        `Failed to fetch AI models for service ${serviceId}: ${parseApiError(error)}`,
      );
    }
  },

  /**
   * Retrieves the current user's AI usage cost for a given time period (in days).
   */
  getUserCost: async (timePeriod: number = 7): Promise<{ TotalCost: number }> => {
    try {
      const response = await apiClient.get<{ TotalCost: number }>(
        '/ai/user-cost',
        { params: { timePeriod } }
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching user cost:', error);
      throw new Error(`Failed to fetch user cost: ${parseApiError(error)}`);
    }
  },
};
