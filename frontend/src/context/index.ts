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

// Custom Theme Context
export {
  CustomThemeProvider,
  useCustomTheme,
} from './CustomThemeContext/CustomThemeContext';
export type { CustomThemeContextType } from './CustomThemeContext/CustomThemeContext.types';

// Re-export ThemeName from its original location
export type { ThemeName } from '../themes';

// Quiz State Context
export {
  QuizStateProvider,
  useQuizState,
} from './QuizStateContext/QuizStateContext';
export type { QuizStateContextType } from './QuizStateContext/QuizStateContext.types';

// Re-export Quiz and SubmissionResponse from their original location
export type { Quiz, SubmissionResponse } from '../types';
