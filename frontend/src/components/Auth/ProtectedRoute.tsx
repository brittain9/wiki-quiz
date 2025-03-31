import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import React, { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';

import useAuthStatus from '../../hooks/useAuthStatus';
import { logAuth } from '../../utils/logger';

interface ProtectedRouteProps {
  /**
   * The component/content to render if authenticated
   */
  children: ReactNode;

  /**
   * Where to redirect if not authenticated
   * @default "/login"
   */
  redirectTo?: string;

  /**
   * Custom loading component
   */
  loadingComponent?: ReactNode;

  /**
   * Reverse the protection (redirect when authenticated)
   * @default false
   */
  redirectIfAuthenticated?: boolean;
}

/**
 * Component that protects routes by checking authentication status
 *
 * @example
 * // Basic usage - protect a route
 * <Route path="/dashboard" element={
 *   <ProtectedRoute>
 *     <DashboardPage />
 *   </ProtectedRoute>
 * } />
 *
 * @example
 * // Redirect authenticated users away from login page
 * <Route path="/login" element={
 *   <ProtectedRoute redirectIfAuthenticated={true} redirectTo="/dashboard">
 *     <LoginPage />
 *   </ProtectedRoute>
 * } />
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  redirectTo = '/login',
  loadingComponent,
  redirectIfAuthenticated = false,
}) => {
  const { isAuthenticated, isLoading } = useAuthStatus();

  // Show loading spinner while checking authentication
  if (isLoading) {
    logAuth('ProtectedRoute: Loading authentication status');
    return (
      loadingComponent || (
        <Box
          display="flex"
          justifyContent="center"
          alignItems="center"
          minHeight="100vh"
        >
          <CircularProgress size={40} />
        </Box>
      )
    );
  }

  // For standard protected routes (redirect to login if not authenticated)
  if (!redirectIfAuthenticated && !isAuthenticated) {
    logAuth('ProtectedRoute: Not authenticated, redirecting', {
      from: window.location.pathname,
      to: redirectTo,
    });
    return <Navigate to={redirectTo} replace />;
  }

  // For login/public pages (redirect authenticated users away)
  if (redirectIfAuthenticated && isAuthenticated) {
    logAuth('ProtectedRoute: Already authenticated, redirecting', {
      from: window.location.pathname,
      to: redirectTo,
    });
    return <Navigate to={redirectTo} replace />;
  }

  // User has appropriate authentication status for this route
  logAuth('ProtectedRoute: Access granted', {
    path: window.location.pathname,
    isAuthenticated,
    redirectIfAuthenticated,
  });

  return <>{children}</>;
};

export default ProtectedRoute;
