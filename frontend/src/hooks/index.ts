// Import hooks
import useAuthActions from './useAuthActions';
import useAuthStatus from './useAuthStatus';
import useProtectedRoute from './useProtectedRoute';
import { useQuizResultOverlay } from './useQuizResultOverlay';

// Re-export hooks
export {
  // Auth
  useAuthStatus,
  useAuthActions,
  useProtectedRoute,

  // UI
  useQuizResultOverlay,
};

// Re-export types
export type { UserInfo } from '../context/AuthContext/AuthContext.types';
export type { Quiz } from '../types/quiz.types';
export type { QuizResult, ResultOption } from '../types/quizResult.types';

// Default export
const hooks = {
  // Auth
  useAuthStatus,
  useAuthActions,
  useProtectedRoute,

  // UI
  useQuizResultOverlay,
};

export default hooks;
