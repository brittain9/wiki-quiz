import axios, {
  AxiosInstance,
  InternalAxiosRequestConfig,
  AxiosResponse,
  AxiosError,
  AxiosRequestConfig,
} from 'axios';

import { logAPI, logError } from '../utils/logger';

// Use an environment variable or configuration setting for the API base URL
const API_BASE_URL = 'http://localhost:5543/api';

/**
 * Configured Axios instance for API calls
 * withCredentials ensures cookies are sent with requests
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // This allows cookies to be sent with cross-origin requests
});

// Request interceptor for logging and token handling
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    // Log outgoing request details
    logAPI(
      `Request: ${config.method?.toUpperCase() || 'UNKNOWN'} ${config.url || 'unknown'}`,
      {
        url: config.url,
        method: config.method,
        headers: config.headers,
        params: config.params,
        withCredentials: config.withCredentials,
        data: config.data
          ? typeof config.data === 'string'
            ? config.data.substring(0, 100) + '...'
            : config.data
          : undefined,
      },
    );

    return config;
  },
  (error: AxiosError) => {
    // Log request error
    logError('Request error', {
      error: error.message,
      code: error.code,
      config: error.config,
    });
    return Promise.reject(error);
  },
);

// Response interceptor for logging and error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    // Log successful response
    logAPI(
      `Response: ${response.status} ${response.config.method?.toUpperCase() || 'UNKNOWN'} ${response.config.url || 'unknown'}`,
      {
        status: response.status,
        statusText: response.statusText,
        headers: response.headers,
        config: {
          url: response.config.url,
          method: response.config.method,
        },
        data: response.data ? 'Data received' : 'No data',
      },
    );

    return response;
  },
  (error: AxiosError) => {
    // Don't log 401s as errors for the /auth/me endpoint since that's expected when not logged in
    if (
      error.config?.url?.includes('/auth/me') &&
      error.response?.status === 401
    ) {
      logAPI('User not authenticated', {
        status: error.response.status,
        endpoint: error.config.url,
      });
    } else {
      // Log response error with details
      logError(`API Error: ${error.response?.status || 'Network Error'}`, {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        method: error.config?.method,
        data: error.response?.data,
      });

      // Special handling for auth errors
      if (error.response && error.response.status === 401) {
        logAPI('Authentication required - not logged in', {
          url: error.config?.url,
          method: error.config?.method,
        });
      }
    }

    return Promise.reject(error);
  },
);

/**
 * Enhanced GET request with logging
 */
export const apiGet = async <T>(
  url: string,
  config?: AxiosRequestConfig,
): Promise<T> => {
  try {
    const response = await apiClient.get<T>(url, config);
    return response.data;
  } catch (error) {
    logError(`GET ${url} failed`, error);
    throw error;
  }
};

/**
 * Enhanced POST request with logging
 */
export const apiPost = async <T, D = unknown>(
  url: string,
  data?: D,
  config?: AxiosRequestConfig,
): Promise<T> => {
  try {
    const response = await apiClient.post<T>(url, data, config);
    return response.data;
  } catch (error) {
    logError(`POST ${url} failed`, error);
    throw error;
  }
};

export default apiClient;
