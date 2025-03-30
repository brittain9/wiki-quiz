// utils.ts - Shared utilities for API calls
import axios, { AxiosError } from 'axios';

interface ApiErrorResponse {
  title?: string;
  detail?: string;
  message?: string;
}

export const parseApiError = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiErrorResponse>;
    if (axiosError.response?.data) {
      return (
        axiosError.response.data.detail ||
        axiosError.response.data.title ||
        axiosError.response.data.message ||
        `API Error: Status ${axiosError.response.status}`
      );
    }
    if (axiosError.code === 'ERR_NETWORK') {
      return 'Network Error: Cannot connect to the API server.';
    }
    // Handle specific known status codes before generic message
    if (axiosError.response?.status === 404) {
      return 'The requested resource was not found.';
    }
    if (axiosError.response?.status === 400) {
      return 'Invalid input provided.';
    }
    return axiosError.message; // Fallback to Axios error message
  }
  // Fallback for non-Axios errors
  return (error as Error)?.message || 'An unexpected error occurred.';
};
