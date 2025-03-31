// Auth hooks
import useAuthActions from './useAuthActions';
import useAuthStatus from './useAuthStatus';
import useProtectedRoute from './useProtectedRoute';

// UI hooks
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
export type { UserInfo } from '../types/auth';
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
