// src/context/AuthProvider.tsx
import { AxiosError } from 'axios';
import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
} from 'react';

import { AuthContext as AuthContextType, UserInfo } from './AuthContext.types';
import { authApi } from '../../services';

// Create context with undefined initial value
const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<React.PropsWithChildren> = ({
  children,
}) => {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [isChecking, setIsChecking] = useState<boolean>(true);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [error, setError] = useState<string | null>(null);

  const clearError = useCallback(() => setError(null), []);

  // Check if user is logged in by fetching their profile
  const checkLoginStatus = useCallback(async () => {
    console.log('Checking login status');
    try {
      // Use the authApi to get the current user
      const userData = await authApi.getCurrentUser();

      setUserInfo(userData);
      setIsLoggedIn(true);
      setError(null);
    } catch (err) {
      const axiosError = err as AxiosError;
      // Don't treat 401 as an error - it just means user isn't logged in
      if (axiosError.response?.status !== 401) {
        console.error('Failed to check login status', err);
      }

      setIsLoggedIn(false);
      setUserInfo(null);
    } finally {
      setIsChecking(false);
    }
  }, []);

  // Effect to run on mount and handle authentication redirects
  useEffect(() => {
    setIsChecking(true);

    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');
    const fromAuthParam = params.get('fromAuth');

    if (errorParam) {
      // Handle specific errors passed back from backend redirect
      const errorMessage =
        errorParam === 'email_exists_different_provider'
          ? 'An account with this email already exists using a different sign-in method.'
          : 'Authentication failed. Please try again.';

      console.error('Authentication error from redirect:', errorParam);

      setError(errorMessage);
      // Clean the URL
      window.history.replaceState({}, document.title, window.location.pathname);
      setIsChecking(false);
    } else {
      // No error param, proceed with normal login check
      checkLoginStatus();
    }
  }, [checkLoginStatus]);

  // Initialize Google OAuth login flow
  const loginWithGoogle = useCallback(() => {
    setError(null);

    authApi.initiateGoogleLogin().catch((err) => {
      console.error('Failed to initiate Google login', err);
      setError('Unable to start Google authentication. Please try again.');
    });
  }, []);

  // Log out the current user
  const logout = useCallback(async () => {
    setError(null);

    try {
      await authApi.logout();

      // Update local state
      setUserInfo(null);
      setIsLoggedIn(false);
    } catch (err) {
      console.error('Logout failed', err);
      setError('Failed to log out. Please try again.');
    }
  }, []);

  // Memoize context value to prevent unnecessary re-renders
  const contextValue = useMemo(
    () => ({
      isLoggedIn,
      isChecking,
      userInfo,
      error,
      loginWithGoogle,
      logout,
      clearError,
    }),
    [
      isLoggedIn,
      isChecking,
      userInfo,
      error,
      loginWithGoogle,
      logout,
      clearError,
    ],
  );

  return (
    <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>
  );
};

// Custom hook to use the auth context
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
