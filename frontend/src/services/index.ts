// Export all API services
export * from './api/authApi';
export * from './api/quizApi';
export * from './api/submissionApi';
export * from './api/wikiApi';
export * from './api/utils';

// Re-export the apiClient for direct usage if needed
export { default as apiClient, apiGet, apiPost } from './apiClient';
