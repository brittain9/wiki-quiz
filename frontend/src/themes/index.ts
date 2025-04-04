import { allThemeIds, loadTheme } from './css';
import { ThemeDefinition } from '../types/themeDefinition';

// Constant array of themes that won't change
export const THEMES: ReadonlyArray<ThemeDefinition> = [
  {
    id: 'light',
    displayName: 'Light',
    previewColors: { main: '#7c3aed', bg: '#f5f5f5', text: '#2d2d2d' },
  },
  {
    id: 'dark',
    displayName: 'Dark Mode',
    previewColors: { main: '#3b82f6', bg: '#121212', text: '#e0e0e0' },
  },
  {
    id: 'serika',
    displayName: 'Serika',
    previewColors: { main: '#e2b714', bg: '#323437', text: '#d1d0c5' },
  },
  {
    id: 'coral',
    displayName: 'Coral',
    previewColors: { main: '#ff9a8b', bg: '#2a2834', text: '#e6e6e6' },
  },
  {
    id: 'solarized',
    displayName: 'Solarized',
    previewColors: { main: '#859900', bg: '#002b36', text: '#93a1a1' },
  },
  {
    id: 'nord',
    displayName: 'Nord',
    previewColors: { main: '#88c0d0', bg: '#2e3440', text: '#d8dee9' },
  },
  {
    id: 'dracula',
    displayName: 'Dracula',
    previewColors: { main: '#bd93f9', bg: '#282a36', text: '#f8f8f2' },
  },
  {
    id: 'monokai',
    displayName: 'Monokai',
    previewColors: { main: '#a6e22e', bg: '#272822', text: '#f8f8f2' },
  },
  {
    id: 'ayu',
    displayName: 'Ayu',
    previewColors: { main: '#ff9940', bg: '#0f1419', text: '#e6e1cf' },
  },
  // New themes
  {
    id: 'cyberpunk',
    displayName: 'Cyberpunk Neon',
    previewColors: { main: '#00ffff', bg: '#0b0b0f', text: '#f0f0f0' },
  },
  {
    id: 'synthwave',
    displayName: 'Synthwave',
    previewColors: { main: '#fe4183', bg: '#241b2f', text: '#f8e3ff' },
  },
  {
    id: 'mint',
    displayName: 'Mint Fresh',
    previewColors: { main: '#0db885', bg: '#f0fffa', text: '#263e39' },
  },
  {
    id: 'bubblegum',
    displayName: 'Bubblegum Pop',
    previewColors: { main: '#ff46b5', bg: '#fff0f7', text: '#4e2e3e' },
  },
  {
    id: 'carbon',
    displayName: 'Carbon Blue',
    previewColors: { main: '#0f62fe', bg: '#161616', text: '#f4f4f4' },
  },
  {
    id: 'sunset',
    displayName: 'Sunset Horizon',
    previewColors: { main: '#ff9e64', bg: '#2d1b2d', text: '#ffecd9' },
  },
  {
    id: 'ocean',
    displayName: 'Deep Ocean',
    previewColors: { main: '#38bdf8', bg: '#0a1929', text: '#e0f2fe' },
  },
  // Second batch of new themes
  {
    id: 'lemonade',
    displayName: 'Lemonade',
    previewColors: { main: '#80b918', bg: '#fdffe3', text: '#2b3829' },
  },
  {
    id: 'galaxy',
    displayName: 'Galaxy',
    previewColors: { main: '#9d4edd', bg: '#10002b', text: '#e0aaff' },
  },
  {
    id: 'coffee',
    displayName: 'Coffee',
    previewColors: { main: '#d0a676', bg: '#1e1717', text: '#ffe8d6' },
  },
  {
    id: 'aquarium',
    displayName: 'Aquarium',
    previewColors: { main: '#5cceee', bg: '#003459', text: '#e6f7fe' },
  },
  {
    id: 'retrogamer',
    displayName: 'Retro Gamer',
    previewColors: { main: '#8bac0f', bg: '#081820', text: '#9bbc0f' },
  },
  {
    id: 'candyland',
    displayName: 'Candyland',
    previewColors: { main: '#fd4db0', bg: '#fcf0fe', text: '#5f2875' },
  },
  {
    id: 'amber',
    displayName: 'Amber Terminal',
    previewColors: { main: '#ffb000', bg: '#2c1c12', text: '#ffd782' },
  },
] as const;

// Precomputed theme IDs (more efficient than recalculating)
export const THEME_IDS = THEMES.map((theme) => theme.id);

// Verify that the theme definitions match CSS files
console.assert(
  allThemeIds.length === THEME_IDS.length,
  'Mismatch between theme definitions and CSS files',
);

// Type representing valid theme IDs
export type ThemeName = (typeof THEME_IDS)[number];

// Theme lookup map for faster theme access by ID
const THEME_MAP = new Map<string, ThemeDefinition>(
  THEMES.map((theme) => [theme.id, theme]),
);

/**
 * Get a theme by its ID
 * @param themeId - The ID of the theme to retrieve
 * @returns The theme definition
 */
export function getTheme(themeId: ThemeName): ThemeDefinition {
  const theme = THEME_MAP.get(themeId);
  if (!theme) {
    console.warn(`Theme not found: ${themeId}, returning light theme`);
    return THEMES[0];
  }
  return theme;
}

/**
 * Get the preview colors for a theme
 * @param themeId - The ID of the theme
 * @returns The preview colors for the theme
 */
export function getThemePreviewColors(themeId: ThemeName): {
  main: string;
  bg: string;
  text: string;
} {
  return getTheme(themeId).previewColors;
}

/**
 * Load a theme's CSS
 * @param themeId - The ID of the theme to load
 */
export function loadThemeCSS(themeId: ThemeName): Promise<any> {
  console.log(`Loading CSS for theme: ${themeId}`);
  return loadTheme(themeId)
    .then((result) => {
      console.log(`Successfully loaded CSS for ${themeId}`);
      return result;
    })
    .catch((error) => {
      console.error(`Error loading CSS for ${themeId}:`, error);
      throw error;
    });
}
