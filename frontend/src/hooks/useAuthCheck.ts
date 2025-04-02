import { useState, useEffect, useCallback } from 'react';

import { useAuth } from '../context/AuthProvider';
import { useOverlay } from '../context/OverlayContext';

interface UseAuthCheckOptions {
  /**
   * Custom message to show in the login dialog
   */
  message?: string;
  
  /**
   * Callback to run after successful authentication
   */
  onAuthSuccess?: () => void;
}

/**
 * Hook to check if user is authenticated and show login overlay if not
 * @returns A function to trigger the auth check
 */
export default function useAuthCheck(options: UseAuthCheckOptions = {}) {
  const { isLoggedIn, isChecking } = useAuth();
  const { showOverlay, setOverlayProps } = useOverlay();
  const [lastCheckResult, setLastCheckResult] = useState<boolean | null>(null);

  // Check authentication and show login overlay if needed
  const checkAuth = useCallback(() => {
    // Wait until auth check is complete
    if (isChecking) {
      return false;
    }

    if (!isLoggedIn) {
      // Show login overlay with custom props
      setOverlayProps({
        message: options.message,
        onSuccess: options.onAuthSuccess,
      });
      showOverlay('login');
      setLastCheckResult(false);
      return false;
    }
    
    setLastCheckResult(true);
    return true;
  }, [isLoggedIn, isChecking, showOverlay, setOverlayProps, options]);

  return {
    checkAuth,
    isAuthenticated: isLoggedIn,
    isChecking,
    lastCheckResult,
  };
} 