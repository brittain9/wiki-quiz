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
let loadStyleLoaderTimeouts: number[] = [];
const loadedThemes = new Set<string>();
const themeLoadPromises = new Map<string, Promise<void>>(); // Track in-progress loads

// Load a theme stylesheet
export async function loadStyle(name: string): Promise<void> {
  return new Promise((resolve) => {
    const themeUrl = `/themes/${name}.css`;
    const currentThemeElement = document.getElementById('currentTheme');
    // If the current theme is already applied, do nothing
    if (
      currentThemeElement &&
      currentThemeElement.getAttribute('href') === themeUrl
    ) {
      resolve();
      return;
    }
    function swapCurrentToNext(): void {
      const current = document.getElementById('currentTheme');
      const next = document.getElementById('nextTheme');
      if (!next) return;
      if (current) current.remove();
      next.id = 'currentTheme';
    }
    // Show loader after short delay
    loadStyleLoaderTimeouts.push(
      setTimeout(() => {
        document.body.classList.add('theme-loading');
      }, 100),
    );
    // Remove any previous next theme link (safety measure)
    document.getElementById('nextTheme')?.remove();
    const headScript = document.getElementById('currentTheme');
    const link = document.createElement('link');
    link.type = 'text/css';
    link.rel = 'stylesheet';
    link.id = 'nextTheme';
    link.onload = (): void => {
      document.body.classList.remove('theme-loading');
      swapCurrentToNext();
      loadStyleLoaderTimeouts.forEach((t) => clearTimeout(t));
      loadStyleLoaderTimeouts = [];
      resolve();
    };
    link.onerror = (e): void => {
      document.body.classList.remove('theme-loading');
      document.getElementById('nextTheme')?.remove();
      loadStyleLoaderTimeouts.forEach((t) => clearTimeout(t));
      loadStyleLoaderTimeouts = [];
      resolve();
    };
    link.href = themeUrl;
    if (headScript === null) {
      document.head.appendChild(link);
    } else {
      headScript.after(link);
    }
  });
}

// Clear custom theme
function clearCustomTheme(): void {
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

  // Clear any custom theme styles applied directly to the element
  clearCustomTheme();

  // Load the theme CSS (will use cache if available)
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

  // Update UI elements that depend on theme colors
  updateThemeElements();

  // Update theme name display in UI
  updateThemeName(isPreview ? themeName : undefined);

  // Update dark mode status based on the effective background color
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

// Simple dark color detection based on luminance
function isColorDark(color: string): boolean {
  if (!color) return true; // Default to dark if color is missing or empty

  // Handle hex values (e.g., #rgb, #rrggbb)
  if (color.startsWith('#')) {
    let hex = color.slice(1);
    if (hex.length === 3) {
      hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    }
    if (hex.length === 6) {
      const r = parseInt(hex.slice(0, 2), 16);
      const g = parseInt(hex.slice(2, 4), 16);
      const b = parseInt(hex.slice(4, 6), 16);
      // Luminance formula
      return r * 0.299 + g * 0.587 + b * 0.114 < 128;
    }
  }

  // Handle rgb/rgba values (e.g., rgb(r, g, b), rgba(r, g, b, a))
  if (color.startsWith('rgb')) {
    const parts = color.match(/\d+/g);
    if (parts && parts.length >= 3) {
      const r = parseInt(parts[0], 10);
      const g = parseInt(parts[1], 10);
      const b = parseInt(parts[2], 10);
      // Luminance formula
      return r * 0.299 + g * 0.587 + b * 0.114 < 128;
    }
  }

  console.warn(
    'Could not determine darkness for color:',
    color,
    '- defaulting to dark.',
  );
  return true;
}

// Update any theme-dependent UI elements
function updateThemeElements(): void {
  // Placeholder for updating charts or other UI elements that need explicit color updates
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

// Preview a theme (debounced)
export function preview(
  themeIdentifier: string,
  customColorsOverride?: string[],
): void {
  debouncedPreview(themeIdentifier, customColorsOverride);
}

// Debounced preview function to prevent rapid theme changes during hover/selection
const debouncedPreview = debounce<(t: string, c?: string[]) => void>(
  250, // Debounce interval in milliseconds
  (themeIdentifier, customColorsOverride) => {
    isPreviewingTheme = true;
    // Apply the theme in preview mode
    void apply(themeIdentifier, customColorsOverride, true);
  },
);

// Clear theme preview and revert to the saved theme
export async function clearPreview(applySavedTheme = true): Promise<void> {
  if (isPreviewingTheme) {
    isPreviewingTheme = false;
    if (applySavedTheme) {
      // Re-apply the theme stored in localStorage
      const savedTheme = localStorage.getItem('theme') || 'dark';
      const customThemeColors = JSON.parse(
        localStorage.getItem('customTheme') || 'null',
      );
      await apply(savedTheme, customThemeColors);
    }
  }
}

// Set the actual theme and save it to localStorage
export async function setTheme(themeName: string): Promise<void> {
  // Apply the theme permanently (not preview)
  await apply(themeName);
  // Save the chosen theme name
  localStorage.setItem('theme', themeName);
  // Clear any custom theme colors if a standard theme is set
  localStorage.removeItem('customTheme');
}

// Initialize theme based on saved preferences on app load
export async function initTheme(): Promise<void> {
  const savedTheme = localStorage.getItem('theme') || 'dark';
  const customThemeColors = JSON.parse(
    localStorage.getItem('customTheme') || 'null',
  );
  // Apply the saved theme or custom theme colors
  await apply(savedTheme, customThemeColors);
}
