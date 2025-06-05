/**
 * Represents authenticated user information
 */
export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  totalPoints: number;
  level: number;
}

/**
 * Auth state interface for context or hooks
 */
export interface AuthState {
  isLoggedIn: boolean; // Whether user is logged in
  isChecking: boolean; // Whether auth state is still being checked
  userInfo: UserInfo | null; // User info if logged in
  error: string | null;
}

/**
 * Auth actions interface for context or hooks
 */
export interface AuthActions {
  loginWithGoogle: () => void;
  logout: () => Promise<void>;
  clearError: () => void;
}

export interface AuthContext extends AuthState, AuthActions {}
