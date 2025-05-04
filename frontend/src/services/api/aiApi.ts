import { apiGet, parseApiError } from '../apiService';

const AI_ENDPOINTS = {
  SERVICES: '/api/ai/services',
  MODELS: '/api/ai/models',
  USER_COST: '/api/ai/user-cost',
} as const;

export const aiApi = {
  /**
   * Gets the available AI services
   */
  async getAiServices(): Promise<string[]> {
    try {
      const response = await apiGet<string[]>(AI_ENDPOINTS.SERVICES);
      return response;
    } catch (error) {
      console.error(`Failed to get AI services: ${parseApiError(error)}`);
      return [];
    }
  },

  /**
   * Gets available models for a specific AI service
   */
  async getAiModels(aiServiceId: string): Promise<string[]> {
    try {
      const response = await apiGet<string[]>(
        `${AI_ENDPOINTS.MODELS}?aiServiceId=${aiServiceId}`,
      );
      return response;
    } catch (error) {
      console.error(
        `Failed to get AI models for service ${aiServiceId}: ${parseApiError(error)}`,
      );
      return [];
    }
  },

  /**
   * Gets the current user's cost for AI usage
   * @param timePeriod - Number of days to check cost for (default: 7)
   */
  async getUserCost(timePeriod: number = 7): Promise<number> {
    try {
      return await apiGet<number>(
        `${AI_ENDPOINTS.USER_COST}?timePeriod=${timePeriod}`,
      );
    } catch (error) {
      console.error(`Failed to get user cost: ${parseApiError(error)}`);
      throw error;
    }
  },
};

export default aiApi;
