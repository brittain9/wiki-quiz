// Export all API services from their modules
export * from './api/quizApi';
export * from './api/submissionApi';
export * from './api/wikiApi';
export * from './api/utils';

// Re-export the apiClient for direct usage if needed
export { default as apiClient } from './apiClient';
