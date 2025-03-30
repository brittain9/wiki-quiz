import axios, {
  AxiosInstance,
  InternalAxiosRequestConfig,
  AxiosResponse,
} from 'axios';

const API_BASE_URL = 'http://localhost:5543/api';

const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // This allows cookies to be sent with requests
});

// Response interceptor for handling common errors
apiClient.interceptors.response.use(
  (response: AxiosResponse): AxiosResponse => {
    return response;
  },
  (error) => {
    // Log auth errors
    if (error.response && error.response.status === 401) {
      console.log('Authentication required - not logged in');
    }
    return Promise.reject(error);
  },
);

export default apiClient;
