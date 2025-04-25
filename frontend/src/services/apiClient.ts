import axios, {
  AxiosInstance,
  InternalAxiosRequestConfig,
  AxiosResponse,
  AxiosError,
  AxiosRequestConfig,
} from 'axios';

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
    if (process.env.NODE_ENV === 'development') {
      console.log(
        `Request: ${config.method?.toUpperCase() || 'UNKNOWN'} ${config.url || 'unknown'}`,
      );
    }

    return config;
  },
  (error: AxiosError) => {
    // Log request error
    console.error('Request error', error.message);
    return Promise.reject(error);
  },
);

// Response interceptor for logging and error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    // Log successful response in development
    if (process.env.NODE_ENV === 'development') {
      console.log(
        `Response: ${response.status} ${response.config.method?.toUpperCase() || 'UNKNOWN'} ${response.config.url || 'unknown'}`,
      );
    }

    return response;
  },
  (error: AxiosError) => {
    // Don't log 401s as errors for the /auth/me endpoint since that's expected when not logged in
    if (
      error.config?.url?.includes('/auth/me') &&
      error.response?.status === 401
    ) {
      // User not authenticated - expected for /auth/me
      if (process.env.NODE_ENV === 'development') {
        console.log('User not authenticated', {
          status: error.response.status,
          endpoint: error.config.url,
        });
      }
    } else {
      // Log response error with details
      console.error(`API Error: ${error.response?.status || 'Network Error'}`, {
        message: error.message,
        url: error.config?.url,
        method: error.config?.method,
      });

      // Special handling for auth errors
      if (error.response && error.response.status === 401) {
        console.log('Authentication required - not logged in');
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
    console.error(`GET ${url} failed`, error);
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
    console.error(`POST ${url} failed`, error);
    throw error;
  }
};

export default apiClient;
