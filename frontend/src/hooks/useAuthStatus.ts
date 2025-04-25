import { useAuth } from '../context/AuthContext/AuthContext';

/**
 * Simple hook to check authentication status
 * Useful for components that only need to know if a user is logged in
 */
export default function useAuthStatus() {
  const { isLoggedIn, isChecking, userInfo } = useAuth();

  const isAuthenticated = isLoggedIn && !!userInfo;

  return { isAuthenticated, isChecking };
}
