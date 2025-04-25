// Authentication Context
export { AuthProvider, useAuth } from './AuthContext/AuthContext';
export type {
  AuthContext,
  AuthState,
  AuthActions,
  UserInfo,
} from './AuthContext/AuthContext.types';

// Quiz Options Context
export {
  QuizOptionsProvider,
  useQuizOptions,
} from './QuizOptionsContext/QuizOptionsContext';
export type {
  QuizOptionsContextType,
  QuizOptions,
} from './QuizOptionsContext/QuizOptionsContext.types';

// Overlay Context
export { OverlayProvider, useOverlay } from './OverlayContext/OverlayContext';
export type {
  OverlayContextType,
  OverlayType,
  OverlayData,
} from './OverlayContext/OverlayContext.types';

// Quiz State Context
export {
  QuizStateProvider,
  useQuizState,
} from './QuizStateContext/QuizStateContext';
export type { QuizStateContextType } from './QuizStateContext/QuizStateContext.types';

export type { Quiz, SubmissionResponse } from '../types';
