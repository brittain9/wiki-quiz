import axios, {
  AxiosInstance,
  InternalAxiosRequestConfig,
  AxiosResponse,
  AxiosError,
  AxiosRequestConfig,
} from 'axios';

// Use environment variables for API URLs
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
console.log('API Base URL:', API_BASE_URL);

// Interfaces for error handling
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
    // Handle specific known status codes
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

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // Enable withCredentials to allow cookies to be sent/received in cross-origin requests
  // This is required for JWT authentication via HTTP-only cookies
  withCredentials: true,
});

// Request interceptor for logging and token handling
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
    // Any request pre-processing can go here
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  },
);

// Response interceptor for logging and error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    return response;
  },
  (error: AxiosError) => {
    // Don't log 401s as errors for the /auth/user endpoint since that's expected when not logged in
    if (
      error.config?.url?.includes('/auth/user') &&
      error.response?.status === 401
    ) {
      // User not authenticated - expected for /auth/user
    } else {
      // Special handling for auth errors
      if (error.response && error.response.status === 401) {
        // Authentication required - not logged in
      }
    }

    return Promise.reject(error);
  },
);

export const apiGet = async <T>(
  url: string,
  config?: AxiosRequestConfig,
): Promise<T> => {
  try {
    const response = await apiClient.get<T>(url, config);
    return response.data;
  } catch (error) {
    console.error(`API GET error for ${url}:`, error);
    throw error;
  }
};

export const apiPost = async <T, D = unknown>(
  url: string,
  data?: D,
  config?: AxiosRequestConfig,
): Promise<T> => {
  try {
    const response = await apiClient.post<T>(url, data, config);
    return response.data;
  } catch (error) {
    throw error;
  }
};

export default apiClient;
