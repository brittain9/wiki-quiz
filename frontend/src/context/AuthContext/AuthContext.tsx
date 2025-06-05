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

import type { UserInfo as ApiUserInfo } from '../../services/api/authApi';

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

  // Refresh token periodically to maintain the session
  // This is typically called when the app loads and then at regular intervals
  const refreshToken = useCallback(async () => {
    if (!isLoggedIn) return;

    try {
      await authApi.refreshToken();
    } catch (err) {
      console.error('Token refresh failed, logging out user', err);
      setIsLoggedIn(false);
      setUserInfo(null);
    }
  }, [isLoggedIn]);

  // Check if user is logged in by fetching their profile
  const checkLoginStatus = useCallback(async () => {
    console.log('Checking login status');
    try {
      // Use the authApi to get the current user
      const userData: ApiUserInfo = await authApi.getCurrentUser();
      // Map API user info to context user info
      const mappedUserInfo: UserInfo = {
        id: userData.id,
        email: userData.email ?? '',
        firstName: userData.firstName ?? '',
        lastName: userData.lastName ?? '',
        totalPoints: userData.totalPoints ?? 0,
        level: userData.level ?? 1,
      };
      setUserInfo(mappedUserInfo);
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
    // Check for returnUrl parameter which is what backend is using
    const returnUrlParam = params.get('returnUrl');

    if (errorParam) {
      // Handle specific errors passed back from backend redirect
      let errorMessage = 'Authentication failed. Please try again.';

      if (errorParam === 'email_exists_different_provider') {
        errorMessage =
          'An account with this email already exists using a different sign-in method.';
      } else if (errorParam === 'google_authentication_failed') {
        errorMessage = 'Google authentication failed. Please try again.';
      } else if (errorParam === 'google_processing_error') {
        errorMessage = 'Error processing Google sign-in. Please try again.';
      }

      console.error('Authentication error from redirect:', errorParam);

      setError(errorMessage);
      // Clean the URL
      window.history.replaceState({}, document.title, window.location.pathname);
      setIsChecking(false);
    } else if (returnUrlParam) {
      // Successfully redirected with a returnUrl parameter
      console.log(
        'User returned from successful authentication with returnUrl',
      );
      checkLoginStatus();
      // Clean the URL
      window.history.replaceState({}, document.title, window.location.pathname);
    } else {
      // No error param, proceed with normal login check
      checkLoginStatus();
    }
  }, [checkLoginStatus]);

  // Set up token refresh interval when logged in
  useEffect(() => {
    if (isLoggedIn) {
      // Refresh token every 15 minutes (900000ms)
      const refreshInterval = setInterval(refreshToken, 900000);
      return () => clearInterval(refreshInterval);
    }
  }, [isLoggedIn, refreshToken]);

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
