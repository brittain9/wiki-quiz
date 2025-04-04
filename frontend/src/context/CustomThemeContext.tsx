import React, { createContext, useContext, useEffect, useState } from 'react';

import { AVAILABLE_THEMES, ThemeName } from '../themes';

interface CustomThemeContextType {
  currentTheme: ThemeName;
  setTheme: (theme: ThemeName) => void;
  previewTheme: (theme: ThemeName | null) => void;
}

const CustomThemeContext = createContext<CustomThemeContextType | undefined>(
  undefined,
);

export const useCustomTheme = (): CustomThemeContextType => {
  const context = useContext(CustomThemeContext);
  if (context === undefined) {
    throw new Error('useCustomTheme must be used within a CustomThemeProvider');
  }
  return context;
};

export const CustomThemeProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [currentTheme, setCurrentTheme] = useState<ThemeName>(() => {
    const savedTheme = localStorage.getItem('quiz-app-theme');
    if (savedTheme && AVAILABLE_THEMES.includes(savedTheme as ThemeName)) {
      return savedTheme as ThemeName;
    }
    return 'default';
  });

  // Apply theme when it changes
  useEffect(() => {
    applyTheme(currentTheme, true);
    console.log(`Theme changed to: ${currentTheme}`);
  }, [currentTheme]);

  const handleSetTheme = (theme: ThemeName) => {
    if (AVAILABLE_THEMES.includes(theme)) {
      setCurrentTheme(theme);
    } else {
      console.warn(`Invalid theme: ${theme}. Using default theme instead.`);
      setCurrentTheme('default');
    }
  };

  // Apply theme function that can be used for both preview and permanent changes
  const applyTheme = (theme: ThemeName, saveToStorage: boolean = false) => {
    // Save theme preference to localStorage if it's a permanent change
    if (saveToStorage) {
      localStorage.setItem('quiz-app-theme', theme);
    }

    // Apply theme by adding class to document root elements
    document.documentElement.className = `theme-${theme}`;
    document.body.className = `theme-${theme}`;

    // Set data attribute for CSS selectors
    document.documentElement.setAttribute('data-theme', theme);
    document.body.setAttribute('data-theme', theme);

    // Dynamically load theme CSS
    const themeLinkId = 'theme-style';
    let themeLink = document.getElementById(themeLinkId) as HTMLLinkElement;

    if (!themeLink) {
      themeLink = document.createElement('link');
      themeLink.id = themeLinkId;
      themeLink.rel = 'stylesheet';
      document.head.appendChild(themeLink);
    }

    // Load theme from public/themes directory with a cache-busting parameter
    themeLink.href = `/themes/${theme}.css?v=${Date.now()}`;
  };

  // Function to preview a theme without saving it
  const previewTheme = (theme: ThemeName | null) => {
    if (theme === null) {
      // Revert to current theme if preview is cleared
      applyTheme(currentTheme);
    } else if (AVAILABLE_THEMES.includes(theme)) {
      applyTheme(theme);
    }
  };

  return (
    <CustomThemeContext.Provider
      value={{
        currentTheme,
        setTheme: handleSetTheme,
        previewTheme,
      }}
    >
      {children}
    </CustomThemeContext.Provider>
  );
};

export default CustomThemeContext;
