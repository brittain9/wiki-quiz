import apiClient from './apiClient';

interface User {
  id: string;
  email: string;
  name: string;
  picture?: string;
}

const authService = {
  // Get the currently authenticated user
  getCurrentUser: async (): Promise<User | null> => {
    try {
      const response = await apiClient.get('/auth/me');
      return response.data;
    } catch (error) {
      console.error('Error getting current user:', error);
      return null; // Return null instead of throwing to avoid redirect loops
    }
  },

  // Logout the user
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      console.error('Error during logout:', error);
    }
  }
};

export default authService; 