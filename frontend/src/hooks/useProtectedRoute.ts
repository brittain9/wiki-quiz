import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import useAuthStatus from './useAuthStatus';

/**
 * Options for the useProtectedRoute hook
 */
interface ProtectedRouteOptions {
  /**
   * The path to redirect to if not authenticated
   * @default "/login"
   */
  redirectTo?: string;

  /**
   * Whether to reverse the protection (redirect if authenticated)
   * @default false
   */
  redirectIfAuthenticated?: boolean;
}

/**
 * Hook to protect routes from unauthenticated users
 *
 * @example
 * // Basic usage - redirect to login if not authenticated
 * function ProtectedComponent() {
 *   const { isLoading } = useProtectedRoute();
 *   if (isLoading) return <Loading />;
 *   return <YourComponent />;
 * }
 *
 * @example
 * // Redirect authenticated users away from login page
 * function LoginPage() {
 *   const { isLoading } = useProtectedRoute({
 *     redirectIfAuthenticated: true,
 *     redirectTo: "/dashboard"
 *   });
 *   if (isLoading) return <Loading />;
 *   return <LoginForm />;
 * }
 */
export default function useProtectedRoute(options?: ProtectedRouteOptions) {
  const { redirectTo = '/login', redirectIfAuthenticated = false } =
    options || {};

  const { isAuthenticated, isChecking } = useAuthStatus();
  const navigate = useNavigate();

  useEffect(() => {
    // Skip while still checking authentication status
    if (isChecking) return;

    // For protected routes - redirect if NOT authenticated
    if (!redirectIfAuthenticated && !isAuthenticated) {
      navigate(redirectTo);
      return;
    }

    // For login/public routes - redirect if authenticated (when enabled)
    if (redirectIfAuthenticated && isAuthenticated) {
      navigate(redirectTo);
      return;
    }
  }, [
    isAuthenticated,
    isChecking,
    navigate,
    redirectIfAuthenticated,
    redirectTo,
  ]);

  return { isLoading: isChecking, isAuthenticated };
}
