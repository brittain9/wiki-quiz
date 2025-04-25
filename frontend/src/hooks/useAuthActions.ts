import { useCallback } from 'react';

import { useAuth } from '../context/AuthContext/AuthContext';

/**
 * Hook to access authentication actions
 * Useful for components that need to trigger login/logout
 */
export default function useAuthActions() {
  const { loginWithGoogle, logout, clearError, error } = useAuth();

  // Enhanced login with Google with additional logging
  const login = useCallback(() => {
    loginWithGoogle();
  }, [loginWithGoogle]);

  return { login, logout, error, clearError };
}
