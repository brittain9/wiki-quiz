/**
 * Represents authenticated user information
 */
export interface UserInfo {
  /**
   * Unique identifier for the user
   */
  id: string;

  /**
   * User's email address
   */
  email: string;

  /**
   * User's first name
   */
  firstName: string;

  /**
   * User's last name
   */
  lastName: string;

  /**
   * User's profile picture URL (from Google account)
   */
  profilePicture?: string;
}

/**
 * Auth state interface for context or hooks
 */
export interface AuthState {
  /**
   * Whether user is logged in
   */
  isLoggedIn: boolean;

  /**
   * Whether auth state is still being checked
   */
  isChecking: boolean;

  /**
   * User information if logged in
   */
  userInfo: UserInfo | null;

  /**
   * Any authentication error
   */
  error: string | null;
}

/**
 * Auth actions interface for context or hooks
 */
export interface AuthActions {
  /**
   * Initiate login with Google OAuth
   */
  loginWithGoogle: () => void;

  /**
   * Log out the current user
   */
  logout: () => Promise<void>;

  /**
   * Clear any authentication errors
   */
  clearError: () => void;
}

/**
 * Complete auth context combining state and actions
 */
export interface AuthContext extends AuthState, AuthActions {}
