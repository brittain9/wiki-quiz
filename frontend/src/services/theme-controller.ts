import { debounce } from 'throttle-debounce';

// Define color variables used in themes
export const colorVars = [
  '--bg-color',
  '--bg-color-secondary',
  '--bg-color-tertiary',
  '--main-color',
  '--caret-color',
  '--sub-color',
  '--sub-alt-color',
  '--text-color',
  '--error-color',
  '--error-extra-color',
  '--success-color',
  '--warning-color',
  '--main-color-10',
  '--main-color-20',
  '--success-color-10',
  '--error-color-10',
];

// Track current state
export const randomTheme: string | null = null;
let isPreviewingTheme = false;
let loadStyleLoaderTimeouts: NodeJS.Timeout[] = [];

// Load a theme stylesheet
export async function loadStyle(name: string): Promise<void> {
  return new Promise((resolve) => {
    console.debug('Loading theme style:', name);

    // Show loader after short delay
    loadStyleLoaderTimeouts.push(
      setTimeout(() => {
        // Show loading indicator
        document.body.classList.add('theme-loading');
      }, 100),
    );

    // Remove any previous next theme
    const nextTheme = document.getElementById('nextTheme');
    if (nextTheme) nextTheme.remove();

    // Create new link element
    const headScript = document.querySelector('#currentTheme');
    const link = document.createElement('link');
    link.type = 'text/css';
    link.rel = 'stylesheet';
    link.id = 'nextTheme';

    // Handle successful load
    link.onload = (): void => {
      console.debug('Theme loaded:', name);
      document.body.classList.remove('theme-loading');

      // Swap current with next theme
      const current = document.getElementById('currentTheme');
      if (current) current.remove();
      link.id = 'currentTheme';

      // Clear timeouts
      loadStyleLoaderTimeouts.forEach((t) => clearTimeout(t));
      loadStyleLoaderTimeouts = [];
      resolve();
    };

    // Handle load error
    link.onerror = (e): void => {
      console.error(`Failed to load theme ${name}`, e);
      document.body.classList.remove('theme-loading');

      // Clear timeouts
      loadStyleLoaderTimeouts.forEach((t) => clearTimeout(t));
      loadStyleLoaderTimeouts = [];
      resolve();
    };

    // Set the href to theme CSS
    link.href = `/themes/${name}.css`;

    // Add to document
    if (headScript === null) {
      document.head.appendChild(link);
    } else {
      headScript.after(link);
    }
  });
}

// Clear custom theme
function clearCustomTheme(): void {
  console.debug('Clearing custom theme');
  for (const e of colorVars) {
    document.documentElement.style.setProperty(e, '');
  }
}

// Update MUI theme mode without changing colors
export function updateMuiThemeMode(isDark: boolean) {
  // Only update the theme mode, not colors
  document.documentElement.setAttribute(
    'data-mui-color-scheme',
    isDark ? 'dark' : 'light',
  );
}

// Apply a theme
export async function apply(
  themeName: string,
  customColorsOverride?: string[],
  isPreview = false,
): Promise<void> {
  console.debug('Applying theme:', themeName, 'preview:', isPreview);

  // Clear any custom theme
  clearCustomTheme();

  // Load the theme CSS
  await loadStyle(themeName);

  // Apply custom colors if needed
  if (customColorsOverride) {
    for (let i = 0; i < colorVars.length; i++) {
      if (i < customColorsOverride.length) {
        document.documentElement.style.setProperty(
          colorVars[i],
          customColorsOverride[i],
        );
      }
    }
  }

  // Update UI elements
  updateThemeElements();

  // Update theme name in UI
  updateThemeName(isPreview ? themeName : undefined);

  // Update dark mode status
  const bgColor = getComputedStyle(document.documentElement)
    .getPropertyValue('--bg-color')
    .trim();
  const isDark = isColorDark(bgColor);

  if (isDark) {
    document.body.classList.add('darkMode');
  } else {
    document.body.classList.remove('darkMode');
  }

  // Update MUI theme mode based on background color
  updateMuiThemeMode(isDark);
}

// Simple dark color detection
function isColorDark(color: string): boolean {
  // Convert hex color to RGB and check luminance
  if (!color) return true; // Default to dark if no color

  // Handle hex values
  if (color.startsWith('#')) {
    const r = parseInt(color.slice(1, 3), 16);
    const g = parseInt(color.slice(3, 5), 16);
    const b = parseInt(color.slice(5, 7), 16);
    return r * 0.299 + g * 0.587 + b * 0.114 < 128;
  }

  // Handle rgb values
  if (color.startsWith('rgb')) {
    const parts = color.match(/\d+/g);
    if (parts && parts.length >= 3) {
      const r = parseInt(parts[0], 10);
      const g = parseInt(parts[1], 10);
      const b = parseInt(parts[2], 10);
      return r * 0.299 + g * 0.587 + b * 0.114 < 128;
    }
  }

  return true; // Default to dark
}

// Update any theme-dependent UI elements
function updateThemeElements(): void {
  // Add code here to update charts or other UI elements
}

// Update theme name display
function updateThemeName(nameOverride?: string): void {
  const themeName = nameOverride || localStorage.getItem('theme') || 'dark';
  const displayName = themeName.replace(/_/g, ' ');
  const themeNameElement = document.querySelector('.current-theme .text');
  if (themeNameElement) {
    themeNameElement.textContent = displayName;
  }
}

// Preview a theme
export function preview(
  themeIdentifier: string,
  customColorsOverride?: string[],
): void {
  debouncedPreview(themeIdentifier, customColorsOverride);
}

// Debounced preview to prevent rapid theme changes
const debouncedPreview = debounce<(t: string, c?: string[]) => void>(
  250,
  (themeIdentifier, customColorsOverride) => {
    isPreviewingTheme = true;
    void apply(themeIdentifier, customColorsOverride, true);
  },
);

// Clear theme preview
export async function clearPreview(applyTheme = true): Promise<void> {
  if (isPreviewingTheme) {
    isPreviewingTheme = false;
    if (applyTheme) {
      const savedTheme = localStorage.getItem('theme') || 'dark';
      const customTheme = JSON.parse(
        localStorage.getItem('customTheme') || 'null',
      );
      await apply(savedTheme, customTheme);
    }
  }
}

// Set the actual theme
export async function setTheme(themeName: string): Promise<void> {
  await apply(themeName);
  localStorage.setItem('theme', themeName);
}

// Initialize theme based on saved preferences
export async function initTheme(): Promise<void> {
  const savedTheme = localStorage.getItem('theme') || 'dark';
  const customTheme = JSON.parse(localStorage.getItem('customTheme') || 'null');
  await apply(savedTheme, customTheme);
}
