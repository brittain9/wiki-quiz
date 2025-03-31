import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import useAuthStatus from './useAuthStatus';
import { logAuth, logNavigation } from '../utils/logger';

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

  const { isAuthenticated, isLoading, user } = useAuthStatus();
  const navigate = useNavigate();

  useEffect(() => {
    // Skip while still checking authentication status
    if (isLoading) return;

    // For protected routes - redirect if NOT authenticated
    if (!redirectIfAuthenticated && !isAuthenticated) {
      handleUnauthenticatedAccess();
      return;
    }

    // For login/public routes - redirect if authenticated (when enabled)
    if (redirectIfAuthenticated && isAuthenticated) {
      handleAuthenticatedAccess();
      return;
    }

    // If we get here, the user can access this route
    logAccessGranted();
  }, [
    isAuthenticated,
    isLoading,
    navigate,
    redirectIfAuthenticated,
    redirectTo,
    user,
  ]);

  /**
   * Handle case where unauthenticated user tries to access protected route
   */
  function handleUnauthenticatedAccess() {
    logAuth('Access to protected route denied - redirecting', {
      redirectTo,
      currentLocation: window.location.pathname,
    });

    logNavigation('Navigating to login page', {
      from: window.location.pathname,
      to: redirectTo,
      reason: 'Authentication required',
    });

    navigate(redirectTo);
  }

  /**
   * Handle case where authenticated user tries to access login/signup page
   */
  function handleAuthenticatedAccess() {
    logAuth('Authenticated user accessing public route - redirecting', {
      redirectTo,
      currentLocation: window.location.pathname,
      user: user
        ? {
            id: user.id,
            email: user.email,
            firstName: user.firstName,
          }
        : null,
    });

    logNavigation('Redirecting authenticated user', {
      from: window.location.pathname,
      to: redirectTo,
      reason: 'Already authenticated',
    });

    navigate(redirectTo);
  }

  /**
   * Log successful access to current route
   */
  function logAccessGranted() {
    logAuth('Route access granted', {
      isAuthenticated,
      path: window.location.pathname,
      user: user
        ? {
            id: user.id,
            email: user.email,
          }
        : null,
    });
  }

  return { isLoading, isAuthenticated, user };
}
