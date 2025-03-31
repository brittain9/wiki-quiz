// frontend/src/context/AuthProvider.tsx
import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
  useMemo,
} from 'react';

const API_BASE_URL = 'http://localhost:5543';
const APP_BASE_URL = 'http://localhost:5173';

interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

interface AuthContextType {
  isLoggedIn: boolean;
  isChecking: boolean;
  userInfo: UserInfo | null;
  error: string | null;
  loginWithGoogle: () => void;
  logout: () => Promise<void>;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [isChecking, setIsChecking] = useState<boolean>(true); // Start checking on load
  const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
  const [error, setError] = useState<string | null>(null);

  const clearError = useCallback(() => setError(null), []);

  const checkLoginStatus = useCallback(async () => {
    try {
      // Use 'no-cache' to ensure freshness, backend should also set appropriate headers
      const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
        credentials: 'include', // Important for sending cookies
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
        },
      });

      if (response.ok) {
        const userData: UserInfo = await response.json();
        setUserInfo(userData);
        setIsLoggedIn(true);
        setError(null); // Clear any previous errors on successful login check
      } else {
        // Consider checking response.status for specific handling (e.g., 401 Unauthorized)
        setIsLoggedIn(false);
        setUserInfo(null);
      }
    } catch (err) {
      console.error('Failed to check login status:', err);
      setIsLoggedIn(false);
      setUserInfo(null);
    } finally {
      setIsChecking(false); // Finished checking
    }
  }, []);

  // Effect to run on initial mount and handle post-redirect logic
  useEffect(() => {
    setIsChecking(true); // Ensure we are in checking state initially
    const params = new URLSearchParams(window.location.search);
    const errorParam = params.get('error');

    if (errorParam) {
      // Handle specific errors passed back from backend redirect
      setError(
        errorParam === 'email_exists_different_provider'
          ? 'An account with this email already exists using a different sign-in method.'
          : 'Authentication failed. Please try again.',
      );
      // Clean the URL
      window.history.replaceState({}, document.title, window.location.pathname);
      setIsChecking(false); // Stop checking if we found an error param
    } else {
      // No error param, proceed to check login status normally
      checkLoginStatus();
    }
  }, [checkLoginStatus]); // checkLoginStatus as dependency

  // Redirects user to backend Google login endpoint
  const loginWithGoogle = useCallback(() => {
    setError(null); // Clear errors before attempting login
    // Add timestamp or state param for potential CSRF mitigation if backend supports it
    const timestamp = new Date().getTime();
    const returnUrl = encodeURIComponent(`${APP_BASE_URL}`); // No need for query params like fromAuth=true now
    window.location.href = `${API_BASE_URL}/api/auth/login/google?returnUrl=${returnUrl}&t=${timestamp}`;
  }, []);

  // Calls backend logout endpoint and updates state
  const logout = useCallback(async () => {
    setError(null); // Clear previous errors
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error('Logout request failed');
      }

      // If logout succeeded on backend (which clears HttpOnly cookie), update frontend state
      setUserInfo(null);
      setIsLoggedIn(false);
    } catch (err) {
      console.error('Logout failed:', err);
      setError('Failed to log out. Please try again.');
    }
  }, []);

  // Memoize context value to prevent unnecessary re-renders of consumers
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

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
