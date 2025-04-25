import { ThemeName } from '../../themes';

export interface CustomThemeContextType {
  /** The theme explicitly chosen by the user (null if using system preference) */
  userSelectedTheme: ThemeName | null;
  /** The current system preferred theme ('light' or 'dark') */
  systemTheme: 'light' | 'dark';
  /** The theme currently being previewed on hover (null if not previewing) */
  previewingTheme: ThemeName | null;
  /** The theme that is currently visually applied (preview > user selection > system) */
  themeToDisplay: ThemeName;
  /** The actual non-preview theme */
  currentAppliedTheme: ThemeName;
  /** Sets the theme as the user's explicit choice and saves it */
  setTheme: (theme: ThemeName) => void;
  /** Switches to using the system preference */
  setSystemPreferenceTheme: () => void;
  /** Checks if a string is a valid ThemeName */
  isValidTheme: (theme: string) => theme is ThemeName;
  /** Temporarily previews a theme, or reverts to the active theme if null */
  previewTheme: (theme: ThemeName | null) => void;
}
