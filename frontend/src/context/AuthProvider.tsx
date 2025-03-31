// frontend/src/context/AuthProvider.tsx
import { AxiosError } from 'axios';
import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
  useMemo,
} from 'react';

import { authApi } from '../services';
import { AuthContext as AuthContextType, UserInfo } from '../types/auth';
import { logAuth, logError } from '../utils/logger';

// Create context with undefined initial value
const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [isChecking, setIsChecking] = useState<boolean>(true);
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [error, setError] = useState<string | null>(null);

  const clearError = useCallback(() => setError(null), []);

  // Check if user is logged in by fetching their profile
  const checkLoginStatus = useCallback(async () => {
    logAuth('Checking login status');
    try {
      // Log cookie information for debugging
      logAuth('Cookies present', {
        cookiesLength: document.cookie.length > 0,
        cookiesParsed: document.cookie
          .split('; ')
          .map((c) => c.split('=')[0])
          .join(', '),
      });

      // Use the authApi to get the current user
      const userData = await authApi.getCurrentUser();

      logAuth('User authenticated successfully', {
        id: userData.id,
        email: userData.email,
        firstName: userData.firstName,
      });

      setUserInfo(userData);
      setIsLoggedIn(true);
      setError(null);
    } catch (err) {
      const axiosError = err as AxiosError;
      // Don't treat 401 as an error - it just means user isn't logged in
      if (axiosError.response?.status !== 401) {
        logError('Failed to check login status', err);
      } else {
        logAuth('User not authenticated', { status: 401 });
      }

      setIsLoggedIn(false);
      setUserInfo(null);
    } finally {
      setIsChecking(false);
    }
  }, []);

  // Effect to run on mount and handle authentication redirects
  useEffect(() => {
    logAuth('AuthProvider mounted, initializing auth check');
    setIsChecking(true);

    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');
    const fromAuthParam = params.get('fromAuth');

    if (fromAuthParam === 'true') {
      logAuth('Redirected from authentication flow', {
        url: window.location.href,
        params: Object.fromEntries([...params.entries()]),
      });
    }

    if (errorParam) {
      // Handle specific errors passed back from backend redirect
      const errorMessage =
        errorParam === 'email_exists_different_provider'
          ? 'An account with this email already exists using a different sign-in method.'
          : 'Authentication failed. Please try again.';

      logAuth('Authentication error from redirect', {
        errorParam,
        errorMessage,
      });

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
    logAuth('Initiating Google login');
    setError(null);

    authApi.initiateGoogleLogin().catch((err) => {
      logError('Failed to initiate Google login', err);
      setError('Unable to start Google authentication. Please try again.');
    });
  }, []);

  // Log out the current user
  const logout = useCallback(async () => {
    logAuth('Initiating logout');
    setError(null);

    try {
      await authApi.logout();

      // Update local state
      logAuth('Logout successful, clearing user state');
      setUserInfo(null);
      setIsLoggedIn(false);

      // Log cookies after logout for debugging
      setTimeout(() => {
        logAuth('Cookies after logout', {
          cookiesLength: document.cookie.length > 0,
          cookiesParsed: document.cookie
            .split('; ')
            .map((c) => c.split('=')[0])
            .join(', '),
        });
      }, 100);
    } catch (err) {
      logError('Logout failed', err);
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
