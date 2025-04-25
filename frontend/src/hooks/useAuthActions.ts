import { useCallback } from 'react';

import { useAuth } from '../context/AuthContext/AuthContext';
import { authApi } from '../services';
import { logAuth } from '../utils/logger';

/**
 * Hook that provides authentication actions
 * Useful for components that need to trigger login/logout
 */
export default function useAuthActions() {
  const { loginWithGoogle, logout, clearError, error } = useAuth();

  // Enhanced login with Google with additional logging
  const login = useCallback(() => {
    logAuth('login action triggered from useAuthActions');
    loginWithGoogle();
  }, [loginWithGoogle]);

  // Enhanced logout with additional logging
  const logoutUser = useCallback(async () => {
    logAuth('logout action triggered from useAuthActions');
    try {
      await logout();
      return true;
    } catch (error) {
      logAuth('logout action failed', { error });
      return false;
    }
  }, [logout]);

  // Wrapped clearError with logging
  const resetError = useCallback(() => {
    if (error) {
      logAuth('clearing auth error', { previousError: error });
      clearError();
    }
  }, [clearError, error]);

  // Token refresh functionality (additional feature)
  const refreshToken = useCallback(async () => {
    logAuth('token refresh triggered from useAuthActions');
    try {
      await authApi.refreshToken();
      logAuth('token refresh successful');
      return true;
    } catch (error) {
      logAuth('token refresh failed', { error });
      return false;
    }
  }, []);

  return {
    login,
    logout: logoutUser,
    clearError: resetError,
    refreshToken,
    error,
  };
}
