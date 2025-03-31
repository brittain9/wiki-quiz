import { logError } from './logger';

/**
 * Type-safe way to store data in localStorage with error handling
 */
export const setLocalStorageItem = <T>(key: string, value: T): boolean => {
  try {
    const serializedValue = JSON.stringify(value);
    localStorage.setItem(key, serializedValue);
    return true;
  } catch (error) {
    logError(`Failed to store item in localStorage: ${key}`, error);
    return false;
  }
};

/**
 * Type-safe way to retrieve data from localStorage with error handling
 */
export const getLocalStorageItem = <T>(key: string, defaultValue: T): T => {
  try {
    const serializedValue = localStorage.getItem(key);

    if (serializedValue === null) {
      return defaultValue;
    }

    return JSON.parse(serializedValue) as T;
  } catch (error) {
    logError(`Failed to retrieve item from localStorage: ${key}`, error);
    return defaultValue;
  }
};

/**
 * Remove an item from localStorage with error handling
 */
export const removeLocalStorageItem = (key: string): boolean => {
  try {
    localStorage.removeItem(key);
    return true;
  } catch (error) {
    logError(`Failed to remove item from localStorage: ${key}`, error);
    return false;
  }
};

/**
 * Clear all items from localStorage with error handling
 */
export const clearLocalStorage = (): boolean => {
  try {
    localStorage.clear();
    return true;
  } catch (error) {
    logError('Failed to clear localStorage', error);
    return false;
  }
};

/**
 * Check if localStorage is available in the current environment
 */
export const isLocalStorageAvailable = (): boolean => {
  try {
    const testKey = '__test_storage__';
    localStorage.setItem(testKey, testKey);
    const result = localStorage.getItem(testKey) === testKey;
    localStorage.removeItem(testKey);
    return result;
  } catch (error) {
    return false;
  }
};
