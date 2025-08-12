export const API_BASE_URL: string = (() => {
  const value = import.meta.env.VITE_API_BASE_URL as string | undefined;
  if (!value || value.trim().length === 0) {
    throw new Error('Missing required environment variable: VITE_API_BASE_URL');
  }
  return value;
})();

export const APP_BASE_URL: string = (() => {
  const value = import.meta.env.VITE_APP_BASE_URL as string | undefined;
  if (!value || value.trim().length === 0) {
    throw new Error('Missing required environment variable: VITE_APP_BASE_URL');
  }
  return value;
})();
