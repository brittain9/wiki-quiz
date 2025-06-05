import axios, { AxiosError } from 'axios';

import { apiGet, apiPost, parseApiError } from '../apiService';

// Environment variables with fallback values for safety
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
const APP_BASE_URL = import.meta.env.VITE_APP_BASE_URL;

const AUTH_ENDPOINTS = {
  LOGIN_GOOGLE: '/auth/login/google',
  LOGOUT: '/auth/logout',
  USER_INFO: '/auth/user',
  REFRESH_TOKEN: '/auth/refresh',
} as const;

export interface UserInfo {
  id: string;
  email: string | null;
  firstName: string | null;
  lastName: string | null;
  totalPoints: number;
  level: number;
}

export const authApi = {
  /**
   * Initiates Google OAuth login flow
   * This redirects to Google's authentication page
   */
  async initiateGoogleLogin(): Promise<void> {
    try {
      console.log('Initiating Google login flow');

      // Make sure APP_BASE_URL is properly defined
      if (!APP_BASE_URL) {
        console.error('APP_BASE_URL environment variable is not defined');
        throw new Error('Application configuration error');
      }

      const returnUrl = encodeURIComponent(APP_BASE_URL);
      const loginUrl = `${API_BASE_URL}${AUTH_ENDPOINTS.LOGIN_GOOGLE}?returnUrl=${returnUrl}`;

      // Redirect to backend's auth endpoint
      window.location.href = loginUrl;
    } catch (error) {
      console.error(`Failed to initiate Google login: ${parseApiError(error)}`);
      throw error;
    }
  },

  /**
   * Gets the current user's profile information
   * Used to check if user is authenticated and get their details
   */
  async getCurrentUser(): Promise<UserInfo> {
    try {
      // Use no-cache headers to ensure we get the latest user state
      return await apiGet<UserInfo>(AUTH_ENDPOINTS.USER_INFO, {
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
        },
      });
    } catch (error) {
      // Don't log 401 errors as they're expected when not logged in
      if (axios.isAxiosError(error)) {
        const axiosError = error as AxiosError;
        if (!axiosError.response || axiosError.response.status !== 401) {
          console.error(`Failed to get current user: ${parseApiError(error)}`);
        }
      } else {
        console.error(`Failed to get current user: ${parseApiError(error)}`);
      }
      throw error;
    }
  },

  /**
   * Logs out the current user by invalidating their session
   */
  async logout(): Promise<void> {
    try {
      await apiPost<void>(AUTH_ENDPOINTS.LOGOUT);
      console.log('User successfully logged out');
    } catch (error) {
      console.error(`Failed to logout: ${parseApiError(error)}`);
      throw error;
    }
  },

  /**
   * Refreshes the authentication token
   * Used to extend the session without requiring re-login
   */
  async refreshToken(): Promise<void> {
    try {
      console.log('Refreshing authentication token');
      await apiPost<void>(AUTH_ENDPOINTS.REFRESH_TOKEN);
      console.log('Token successfully refreshed');
    } catch (error) {
      console.error(`Failed to refresh token: ${parseApiError(error)}`);
      throw error;
    }
  },
};

export default authApi;
