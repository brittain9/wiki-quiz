/**
 * Authentication API service
 * Handles all auth-related API calls
 */

import { AxiosError } from 'axios';

import { UserInfo } from '../../context/AuthContext/AuthContext.types';
import { apiGet, apiPost } from '../apiClient';

// API base URLs - should match the backend
const API_BASE_URL = 'http://localhost:5543';
const APP_BASE_URL = 'http://localhost:5173';

// Auth API endpoints - without /api prefix since apiClient already includes it
const AUTH_ENDPOINTS = {
  LOGIN_GOOGLE: '/auth/login/google',
  LOGOUT: '/auth/logout',
  PROFILE: '/auth/me',
  REFRESH_TOKEN: '/auth/refresh',
} as const;

/**
 * Authentication service for handling all auth-related API requests
 */
export const authApi = {
  /**
   * Initiates Google OAuth login flow
   * This typically redirects to Google's authentication page
   */
  async initiateGoogleLogin(): Promise<void> {
    try {
      console.log('Initiating Google login flow');

      // Add timestamp for CSRF mitigation and use return URL parameter
      const timestamp = new Date().getTime();
      const returnUrl = encodeURIComponent(APP_BASE_URL);
      const loginUrl = `${API_BASE_URL}/api${AUTH_ENDPOINTS.LOGIN_GOOGLE}?returnUrl=${returnUrl}&t=${timestamp}`;

      // Directly redirect to Google's authentication page
      // This matches how the backend is set up with the challenge response
      window.location.href = loginUrl;
    } catch (error) {
      console.error('Failed to initiate Google login', error);
      throw new Error('Unable to start Google authentication');
    }
  },

  /**
   * Gets the current user's profile information
   * Used to check if user is authenticated and get their details
   */
  async getCurrentUser(): Promise<UserInfo> {
    try {
      // Use no-cache headers to ensure we get the latest user state
      const response = await apiGet<UserInfo>(AUTH_ENDPOINTS.PROFILE, {
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
        },
      });

      return response;
    } catch (error) {
      const axiosError = error as AxiosError;
      // Don't log 401 errors as they're expected when not logged in
      if (axiosError.response?.status !== 401) {
        console.error('Failed to get current user', error);
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
      console.error('Failed to logout', error);
      throw error;
    }
  },

  /**
   * Refreshes the authentication token
   * Used to extend the session without requiring re-login
   */
  async refreshToken(): Promise<void> {
    try {
      await apiPost<void>(AUTH_ENDPOINTS.REFRESH_TOKEN);
    } catch (error) {
      console.error('Failed to refresh token', error);
      throw error;
    }
  },
};

export default authApi;
