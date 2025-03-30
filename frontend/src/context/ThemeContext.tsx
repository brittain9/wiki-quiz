import { PaletteMode, useMediaQuery, CssBaseline } from '@mui/material';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import React, {
  createContext,
  useContext,
  useState,
  useMemo,
  useEffect,
} from 'react';

import { lightTheme, darkTheme } from '../theme';

type ThemeContextType = {
  mode: PaletteMode;
  toggleColorMode: () => void;
  systemPrefersDark: boolean;
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  // Check if system prefers dark mode
  const systemPrefersDark = useMediaQuery('(prefers-color-scheme: dark)');

  // Try to get the theme from localStorage first, then system preference, defaulting to 'light'
  const [mode, setMode] = useState<PaletteMode>(() => {
    const savedMode = localStorage.getItem('theme-mode');
    if (savedMode === 'light' || savedMode === 'dark') {
      return savedMode as PaletteMode;
    }
    return systemPrefersDark ? 'dark' : 'light';
  });

  // Update theme if system preference changes and no saved preference exists
  useEffect(() => {
    if (!localStorage.getItem('theme-mode')) {
      setMode(systemPrefersDark ? 'dark' : 'light');
    }
  }, [systemPrefersDark]);

  // Save theme preference to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem('theme-mode', mode);
  }, [mode]);

  const toggleColorMode = () => {
    setMode((prevMode) => (prevMode === 'dark' ? 'light' : 'dark'));
  };

  // Memoize the theme to prevent unnecessary re-renders
  const theme = useMemo(() => {
    return mode === 'dark' ? darkTheme : lightTheme;
  }, [mode]);

  const contextValue = useMemo(() => {
    return { mode, toggleColorMode, systemPrefersDark };
  }, [mode, systemPrefersDark]);

  return (
    <ThemeContext.Provider value={contextValue}>
      <MuiThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </MuiThemeProvider>
    </ThemeContext.Provider>
  );
};
