import { createTheme, ThemeOptions } from '@mui/material/styles';

// Create a MUI theme WITHOUT colors (just typography, spacing, etc.)
export function createAppTheme(): ThemeOptions {
  return {
    // Typography
    typography: {
      fontFamily: 'var(--font-family-main)',
      h1: {
        fontWeight: 600,
        fontSize: '2.5rem',
      },
      h2: {
        fontWeight: 600,
        fontSize: '2rem',
      },
      h3: {
        fontWeight: 600,
        fontSize: '1.75rem',
      },
      h4: {
        fontWeight: 600,
        fontSize: '1.5rem',
      },
      h5: {
        fontWeight: 600,
        fontSize: '1.25rem',
      },
      h6: {
        fontWeight: 600,
        fontSize: '1rem',
      },
      subtitle1: {
        fontSize: '1rem',
      },
      subtitle2: {
        fontSize: '0.875rem',
      },
      body1: {
        fontSize: '1rem',
      },
      body2: {
        fontSize: '0.875rem',
      },
      button: {
        fontWeight: 500,
        textTransform: 'none',
      },
    },

    // Spacing, breakpoints, etc.
    spacing: 8,
    shape: {
      borderRadius: 8,
    },

    // Component overrides that aren't color related
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            textTransform: 'none',
            borderRadius: '8px',
            padding: '8px 16px',
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            borderRadius: '8px',
          },
        },
      },
      MuiAppBar: {
        styleOverrides: {
          root: {
            boxShadow: 'none',
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            borderRadius: '12px',
            overflow: 'hidden',
          },
        },
      },
      MuiTextField: {
        styleOverrides: {
          root: {
            borderRadius: '8px',
          },
        },
      },
    },

    // Empty palette - will use CSS variables instead
    palette: {
      // Minimal structure to satisfy TypeScript
      mode: 'dark',
      primary: { main: '#000000' }, // These will be overwritten by CSS vars
      secondary: { main: '#000000' },
      error: { main: '#000000' },
      warning: { main: '#000000' },
      success: { main: '#000000' },
      info: { main: '#000000' },
      background: {
        default: '#000000',
        paper: '#000000',
      },
      text: {
        primary: '#000000',
        secondary: '#000000',
      },
    },
  };
}
