// authApi.ts
import apiClient from '../apiClient';
import { parseApiError } from './utils';

export interface User {
  id: string;
  email: string;
  name: string;
  picture?: string;
}

export const authApi = {
  /**
   * Get the currently authenticated user
   */
  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await apiClient.get<User>('/auth/me');
      return response.data;
    } catch (error) {
      console.error('Error getting current user:', error);
      return null; // Return null instead of throwing to avoid redirect loops
    }
  },

  /**
   * Logout the user
   */
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      console.error('Error during logout:', error);
      throw new Error(`Logout failed: ${parseApiError(error)}`);
    }
  },
};
