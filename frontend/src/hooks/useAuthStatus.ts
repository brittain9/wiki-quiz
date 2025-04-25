import { useAuth } from '../context/AuthContext/AuthContext';
import { logAuth } from '../utils/logger';

/**
 * Hook that provides just authentication status information
 * Useful for components that only need to know if a user is logged in
 */
export default function useAuthStatus() {
  const { isLoggedIn, isChecking, userInfo } = useAuth();

  const isAuthenticated = isLoggedIn && !!userInfo;
  const isLoading = isChecking;

  // Log hook usage
  logAuth('useAuthStatus hook used', {
    isAuthenticated,
    isLoading,
    hasUserInfo: !!userInfo,
  });

  return {
    isAuthenticated,
    isLoading,
    user: userInfo,
  };
}
