import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  useMemo,
} from 'react';

import { CustomThemeContextType } from './CustomThemeContext.types';
import {
  ThemeName,
  THEME_IDS,
  loadThemeCSS,
  getThemePreviewColors,
} from '../../themes';

/**
 * Gets the current system color scheme preference.
 * @returns 'light' or 'dark'
 */
const getSystemPreference = (): 'light' | 'dark' => {
  if (
    window.matchMedia &&
    window.matchMedia('(prefers-color-scheme: dark)').matches
  ) {
    return 'dark';
  }
  return 'light';
};

// Create the context with default/initial values
const CustomThemeContext = createContext<CustomThemeContextType>({
  userSelectedTheme: null,
  systemTheme: getSystemPreference(),
  previewingTheme: null,
  themeToDisplay: getSystemPreference(),
  currentAppliedTheme: getSystemPreference(),
  setTheme: () => {},
  setSystemPreferenceTheme: () => {},
  isValidTheme: (theme: string): theme is ThemeName => false,
  previewTheme: () => {},
});

// Hook for components to use the theme context
export const useCustomTheme = () => useContext(CustomThemeContext);

// Provider component
export const CustomThemeProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  // State for system preference
  const [systemTheme, setSystemTheme] = useState<'light' | 'dark'>(
    getSystemPreference,
  );

  // State for user's explicit choice (from localStorage)
  const [userSelectedTheme, setUserSelectedTheme] = useState<ThemeName | null>(
    () => {
      const savedTheme = localStorage.getItem('theme');
      if (savedTheme && THEME_IDS.includes(savedTheme as ThemeName)) {
        return savedTheme as ThemeName;
      }
      return null; // No valid theme saved, use null
    },
  );

  // State for theme preview
  const [previewingTheme, setPreviewingTheme] = useState<ThemeName | null>(
    null,
  );

  // Type guard for theme validation
  const isValidTheme = useCallback((theme: string): theme is ThemeName => {
    return THEME_IDS.includes(theme as ThemeName);
  }, []);

  // ---- Core Logic ----

  // Determine the *actual* theme applied (ignoring preview)
  const currentAppliedTheme = useMemo(() => {
    return userSelectedTheme ?? systemTheme;
  }, [userSelectedTheme, systemTheme]);

  // Determine the theme to *display* (preview takes precedence)
  const themeToDisplay = useMemo(() => {
    return previewingTheme ?? currentAppliedTheme;
  }, [previewingTheme, currentAppliedTheme]);

  // Function to apply ONLY the preview color variables
  const applyPreviewVariables = useCallback(
    (themeId: ThemeName | null) => {
      const targetTheme = themeId ?? currentAppliedTheme;
      console.log(`Applying preview variables for: ${targetTheme}`);
      try {
        const colors = getThemePreviewColors(targetTheme);
        const root = document.documentElement;
        // --- Key color variables to override ---
        // Adjust these variable names if your CSS uses different ones
        root.style.setProperty('--main-color', colors.main);
        root.style.setProperty('--bg-color', colors.bg);
        root.style.setProperty('--text-color', colors.text);
        // Potentially add others like --sub-color, --bg-secondary etc. if needed for good preview
        // Example:
        // root.style.setProperty('--bg-color-secondary', colors.bg); // Simple fallback
        // root.style.setProperty('--sub-color', colors.text); // Simple fallback

        // Apply the target theme class name *as well* to potentially catch simple rules
        document.documentElement.className = targetTheme;
        document.body.className = targetTheme;
      } catch (error) {
        console.error(
          `Failed to apply preview variables for ${targetTheme}:`,
          error,
        );
      }
    },
    [currentAppliedTheme],
  ); // Depends on the *actual* theme for reversion

  // Function to apply FULL theme styles (CSS class + load CSS file)
  const applyFullThemeStyles = useCallback(async (themeId: ThemeName) => {
    console.log(`Applying FULL styles for theme: ${themeId}`);
    try {
      // Remove temporary preview styles if any were applied
      const root = document.documentElement;
      root.style.removeProperty('--main-color');
      root.style.removeProperty('--bg-color');
      root.style.removeProperty('--text-color');

      // Load the theme CSS (even if preloaded, await its readiness/handle errors)
      await loadThemeCSS(themeId);

      // Apply theme class to document and body AFTER successful load
      document.documentElement.className = themeId;
      document.body.className = themeId;

      console.log(`Full theme styles for ${themeId} applied successfully`);
    } catch (error) {
      console.error(`Failed to apply full styles for theme ${themeId}:`, error);
      // Fallback logic remains the same
      if (themeId !== 'light') {
        try {
          await loadThemeCSS('light'); // Ensure fallback CSS is loaded
          document.documentElement.className = 'light';
          document.body.className = 'light';
          console.log('Fell back to light theme styles.');
        } catch (fallbackError) {
          console.error('Failed to apply fallback light theme:', fallbackError);
        }
      }
    }
  }, []);

  // Effect to apply styles based on preview state or actual theme change
  useEffect(() => {
    if (previewingTheme) {
      applyPreviewVariables(previewingTheme);
    } else {
      applyFullThemeStyles(currentAppliedTheme);
    }
  }, [
    previewingTheme,
    currentAppliedTheme,
    applyPreviewVariables,
    applyFullThemeStyles,
  ]);

  // Effect to listen for system preference changes
  useEffect(() => {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    const handleChange = (e: MediaQueryListEvent) => {
      const newSystemTheme = e.matches ? 'dark' : 'light';
      console.log(`System preference changed to: ${newSystemTheme}`);
      setSystemTheme(newSystemTheme);
      // No need to call setCurrentTheme here, activeTheme derivation handles it
    };

    // Set initial system theme state correctly
    setSystemTheme(mediaQuery.matches ? 'dark' : 'light');

    mediaQuery.addEventListener('change', handleChange);
    return () => mediaQuery.removeEventListener('change', handleChange);
  }, []); // Empty dependency array ensures this runs once on mount

  // Effect to EAGERLY LOAD all themes on initial mount
  useEffect(() => {
    console.log('Eagerly loading all theme CSS...');
    THEME_IDS.forEach((themeId) => {
      loadThemeCSS(themeId)
        .then(() => {
          console.log(`Eagerly loaded: ${themeId}`);
        })
        .catch((error) => {
          console.warn(`Failed to eagerly load theme ${themeId}:`, error);
        });
    });
  }, []); // Empty dependency array ensures this runs only once on mount

  // ---- Context API Functions ----

  // Set theme as the user's choice
  const setTheme = useCallback(
    (newTheme: ThemeName) => {
      if (isValidTheme(newTheme)) {
        console.log(`Setting user theme to: ${newTheme}`);
        // Clear preview *before* setting the theme
        // This ensures the useEffect above applies the *full* new theme
        setPreviewingTheme(null);
        setUserSelectedTheme(newTheme);
        localStorage.setItem('theme', newTheme);
      } else {
        console.warn(`Invalid theme provided to setTheme: ${newTheme}`);
      }
    },
    [isValidTheme],
  );

  // Switch to using system preference
  const setSystemPreferenceTheme = useCallback(() => {
    console.log('Setting theme to use system preference');
    // Clear preview *before* setting the theme
    setPreviewingTheme(null);
    setUserSelectedTheme(null);
    localStorage.removeItem('theme');
  }, []);

  // Preview a theme temporarily - Now just updates state
  const previewTheme = useCallback(
    (themeToPreview: ThemeName | null) => {
      if (themeToPreview === null) {
        console.log(
          'Preview cleared - Reverting to actual theme shortly via useEffect',
        );
        setPreviewingTheme(null);
      } else if (isValidTheme(themeToPreview)) {
        console.log(
          `Previewing theme: ${themeToPreview} - Applying preview variables shortly via useEffect`,
        );
        setPreviewingTheme(themeToPreview);
      } else {
        console.warn(
          `Invalid theme provided to previewTheme: ${themeToPreview}`,
        );
      }
    },
    [isValidTheme],
  );

  // Context value to provide
  const value = useMemo(
    () => ({
      userSelectedTheme,
      systemTheme,
      previewingTheme,
      themeToDisplay, // The theme currently rendered (preview or actual)
      currentAppliedTheme, // The actual selected/system theme (no preview)
      setTheme,
      setSystemPreferenceTheme,
      isValidTheme,
      previewTheme,
    }),
    [
      userSelectedTheme,
      systemTheme,
      previewingTheme,
      themeToDisplay,
      currentAppliedTheme,
      setTheme,
      setSystemPreferenceTheme,
      isValidTheme,
      previewTheme,
    ],
  );

  return (
    <CustomThemeContext.Provider value={value}>
      {children}
    </CustomThemeContext.Provider>
  );
};

export default CustomThemeContext;
