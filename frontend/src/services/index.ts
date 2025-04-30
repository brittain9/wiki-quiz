export * from './api/aiApi';
export * from './api/authApi';
export * from './api/quizApi';
export * from './api/submissionApi';
export * from './api/wikiApi';

// Re-export the apiClient for direct usage if needed
export { 
  default as apiClient, 
  apiGet, 
  apiPost, 
  parseApiError 
} from './apiService';
