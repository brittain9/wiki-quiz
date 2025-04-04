/**
 * Theme CSS exports
 * This file makes it easier to manage all themes
 */

// Theme grouping for organization

// Original themes
export const originalThemes = [
  'light',
  'dark',
  'serika',
  'coral',
  'solarized',
  'nord',
  'dracula',
  'monokai',
  'ayu',
];

// Vibrant themes
export const vibrantThemes = [
  'cyberpunk',
  'synthwave',
  'mint',
  'bubblegum',
  'carbon',
  'sunset',
  'ocean',
];

// Fun themes
export const funThemes = [
  'lemonade',
  'galaxy',
  'coffee',
  'aquarium',
  'retrogamer',
  'candyland',
  'amber',
];

// All theme IDs combined
export const allThemeIds = [...originalThemes, ...vibrantThemes, ...funThemes];

// Define themes directory for imports
const themesDir = './';

// Export helper function to load a theme
export const loadTheme = (themeId: string) => {
  console.log(`Loading theme CSS file for: ${themeId}`);
  try {
    // This pattern is supported by webpack - see examples in webpack documentation
    // https://webpack.js.org/api/module-methods/#dynamic-expressions-in-import
    return import(`./${themeId}.css`).then((module) => {
      console.log(`Successfully imported CSS for ${themeId}`);
      return module;
    });
  } catch (error) {
    console.error(`Error importing CSS for ${themeId}:`, error);
    throw error;
  }
};
