// Export all available themes
export const AVAILABLE_THEMES = [
  'default',
  'serika',
  'coral',
  'light',
  'solarized',
  'nord',
  'dracula',
  'monokai',
  'ayu',
] as const;

// Theme display names (for UI)
export const THEME_DISPLAY_NAMES: Record<ThemeName, string> = {
  default: 'Default',
  serika: 'Serika',
  coral: 'Coral',
  light: 'Light',
  solarized: 'Solarized',
  nord: 'Nord',
  dracula: 'Dracula',
  monokai: 'Monokai',
  ayu: 'Ayu',
};

// Theme preview colors for theme selector
export const THEME_PREVIEW_COLORS: Record<
  ThemeName,
  { main: string; bg: string; text: string }
> = {
  default: { main: '#e2b714', bg: '#323437', text: '#d1d0c5' },
  serika: { main: '#e2b714', bg: '#323437', text: '#d1d0c5' },
  coral: { main: '#ff9a8b', bg: '#2a2834', text: '#e6e6e6' },
  light: { main: '#7c3aed', bg: '#f5f5f5', text: '#2d2d2d' },
  solarized: { main: '#859900', bg: '#002b36', text: '#93a1a1' },
  nord: { main: '#88c0d0', bg: '#2e3440', text: '#d8dee9' },
  dracula: { main: '#bd93f9', bg: '#282a36', text: '#f8f8f2' },
  monokai: { main: '#a6e22e', bg: '#272822', text: '#f8f8f2' },
  ayu: { main: '#ff9940', bg: '#0f1419', text: '#e6e1cf' },
};

export type ThemeName = (typeof AVAILABLE_THEMES)[number];
