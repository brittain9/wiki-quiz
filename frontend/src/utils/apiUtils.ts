import { AxiosError } from 'axios';

import { logError } from './logger';

/**
 * Extracts a human-readable error message from an API error
 */
export const getErrorMessage = (error: unknown): string => {
  if (error instanceof AxiosError) {
    // Handle Axios errors
    if (error.response) {
      // Server responded with an error status
      const data = error.response.data;

      if (typeof data === 'string') {
        return data;
      }

      if (data && typeof data === 'object') {
        // Try to extract error message from common API error formats
        if ('message' in data && typeof data.message === 'string') {
          return data.message;
        }

        if ('error' in data && typeof data.error === 'string') {
          return data.error;
        }

        if ('errorMessage' in data && typeof data.errorMessage === 'string') {
          return data.errorMessage;
        }
      }

      return `API Error: ${error.response.status} ${error.response.statusText}`;
    }

    if (error.request) {
      // Request was made but no response received
      return 'Network error. Please check your internet connection.';
    }

    // Something else happened while setting up the request
    return error.message || 'An unexpected error occurred';
  }

  // Handle non-Axios errors
  if (error instanceof Error) {
    return error.message;
  }

  return 'An unknown error occurred';
};

/**
 * General error handler for API requests
 * Logs the error and returns a human-readable message
 */
export const handleApiError = (error: unknown, context?: string): string => {
  // Log the error with additional context
  logError(`API error${context ? ` in ${context}` : ''}`, error);

  // Get human-readable message
  return getErrorMessage(error);
};
