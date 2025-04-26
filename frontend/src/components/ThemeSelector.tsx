import CheckIcon from '@mui/icons-material/Check';
import PaletteIcon from '@mui/icons-material/Palette';
import {
  Box,
  Typography,
  Fab,
  CircularProgress,
  Popper,
  Paper,
  ClickAwayListener,
  Grow,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
} from '@mui/material';
import React, { useRef, useEffect } from 'react';

import * as ThemeController from '../services/theme-controller';

interface Theme {
  id: string;
  name: string;
  color: string;
  description: string;
}

const themes: Theme[] = [
  {
    id: 'dark',
    name: 'Dark',
    color: '#7c3aed',
    description: 'Modern dark theme with purple accents',
  },
  {
    id: 'light',
    name: 'Light',
    color: '#6d28d9',
    description: 'Clean light theme with violet accents',
  },
  {
    id: 'nature',
    name: 'Nature',
    color: '#86c98f',
    description: 'Forest-inspired dark green theme',
  },
  {
    id: 'contrast',
    name: 'High Contrast',
    color: '#ffcc00',
    description: 'Maximum contrast for accessibility',
  },
];

// Main theme selector component with floating button and dropdown
const ThemeSelector: React.FC = () => {
  const [currentTheme, setCurrentTheme] = React.useState<string>(
    localStorage.getItem('theme') || 'dark',
  );
  const [open, setOpen] = React.useState(false);
  const [previewingTheme, setPreviewingTheme] = React.useState<string | null>(
    null,
  );
  const [loading, setLoading] = React.useState(false);
  const anchorRef = useRef<HTMLDivElement>(null);
  const previewTimerRef = useRef<number | null>(null);

  useEffect(() => {
    // Load the theme from localStorage on component mount
    const savedTheme = localStorage.getItem('theme') || 'dark';
    setCurrentTheme(savedTheme);

    // Clear any preview timers on unmount
    return () => {
      if (previewTimerRef.current) {
        clearTimeout(previewTimerRef.current);
      }
    };
  }, []);

  const handleToggleMenu = () => {
    setOpen((prevOpen) => !prevOpen);
  };

  const handleCloseMenu = (event: Event | React.SyntheticEvent) => {
    if (
      anchorRef.current &&
      anchorRef.current.contains(event.target as HTMLElement)
    ) {
      return;
    }

    handleClearPreview();
    setOpen(false);
  };

  const handleThemeChange = async (themeId: string) => {
    setLoading(true);
    await ThemeController.setTheme(themeId);
    setCurrentTheme(themeId);
    setPreviewingTheme(null);
    setLoading(false);
    setOpen(false);
  };

  const handleThemePreview = (themeId: string) => {
    // Clear any existing preview timer
    if (previewTimerRef.current) {
      clearTimeout(previewTimerRef.current);
    }

    // Set a timer to preview the theme after 250ms
    previewTimerRef.current = setTimeout(() => {
      ThemeController.preview(themeId);
      setPreviewingTheme(themeId);
    }, 250);
  };

  const handleClearPreview = () => {
    // Clear any existing preview timer
    if (previewTimerRef.current) {
      clearTimeout(previewTimerRef.current);
      previewTimerRef.current = null;
    }

    ThemeController.clearPreview();
    setPreviewingTheme(null);
  };

  // Floating theme button
  const floatingButton = (
    <Box
      ref={anchorRef}
      sx={{
        position: 'fixed',
        bottom: 20,
        right: 20,
        zIndex: 1050,
      }}
    >
      <Fab
        color="primary"
        aria-label="theme"
        onClick={handleToggleMenu}
        aria-haspopup="true"
        aria-controls={open ? 'theme-menu' : undefined}
        aria-expanded={open ? 'true' : undefined}
        sx={{
          bgcolor: 'var(--main-color)',
          color: 'var(--bg-color)',
          '&:hover': {
            bgcolor: 'var(--caret-color)',
          },
        }}
      >
        {loading ? (
          <CircularProgress size={24} sx={{ color: 'var(--bg-color)' }} />
        ) : (
          <PaletteIcon />
        )}
      </Fab>
    </Box>
  );

  // Theme selection dropdown menu
  const themeMenu = (
    <Popper
      open={open}
      anchorEl={anchorRef.current}
      placement="top-end"
      transition
      disablePortal
      style={{ zIndex: 1050 }}
    >
      {({ TransitionProps }) => (
        <Grow {...TransitionProps} style={{ transformOrigin: 'center bottom' }}>
          <Paper
            elevation={6}
            sx={{
              width: 280,
              maxHeight: 'calc(100vh - 100px)',
              overflow: 'auto',
              bgcolor: 'var(--bg-color)',
              color: 'var(--text-color)',
              borderRadius: 2,
              mb: 1,
              border: '1px solid var(--sub-alt-color)',
            }}
          >
            <ClickAwayListener onClickAway={handleCloseMenu}>
              <Box>
                <Typography
                  variant="subtitle1"
                  sx={{
                    fontWeight: 'bold',
                    p: 2,
                    pb: 1,
                    borderBottom: '1px solid var(--sub-alt-color)',
                  }}
                >
                  Select Theme
                </Typography>
                <List sx={{ pt: 0 }}>
                  {themes.map((theme, index) => (
                    <React.Fragment key={theme.id}>
                      {index > 0 && (
                        <Divider
                          variant="middle"
                          sx={{ bgcolor: 'var(--sub-alt-color)', opacity: 0.5 }}
                        />
                      )}
                      <ListItem
                        onClick={() => handleThemeChange(theme.id)}
                        onMouseEnter={() => handleThemePreview(theme.id)}
                        onMouseLeave={handleClearPreview}
                        sx={{
                          px: 2,
                          py: 1.5,
                          cursor: 'pointer',
                          backgroundColor:
                            currentTheme === theme.id
                              ? 'var(--main-color-10)'
                              : 'transparent',
                          '&:hover': {
                            bgcolor: 'var(--bg-color-secondary)',
                          },
                        }}
                      >
                        <Box
                          sx={{
                            width: 24,
                            height: 24,
                            borderRadius: '50%',
                            bgcolor: theme.color,
                            border: '2px solid var(--sub-alt-color)',
                            mr: 2,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                          }}
                        >
                          {currentTheme === theme.id && (
                            <CheckIcon
                              sx={{
                                fontSize: 16,
                                color: isContrastingWithBackground(theme.color)
                                  ? '#fff'
                                  : '#000',
                              }}
                            />
                          )}
                        </Box>
                        <ListItemText
                          primary={
                            <Typography
                              sx={{
                                fontWeight:
                                  currentTheme === theme.id ? 'bold' : 'normal',
                              }}
                            >
                              {theme.name}
                              {previewingTheme === theme.id && (
                                <Typography
                                  component="span"
                                  variant="caption"
                                  sx={{
                                    ml: 1,
                                    color: 'var(--main-color)',
                                    fontStyle: 'italic',
                                  }}
                                >
                                  (Previewing)
                                </Typography>
                              )}
                            </Typography>
                          }
                          secondary={
                            <Typography
                              variant="body2"
                              sx={{ color: 'var(--sub-color)', mt: 0.5 }}
                            >
                              {theme.description}
                            </Typography>
                          }
                        />
                      </ListItem>
                    </React.Fragment>
                  ))}
                </List>
              </Box>
            </ClickAwayListener>
          </Paper>
        </Grow>
      )}
    </Popper>
  );

  // Element to store the current theme name for ThemeController
  const themeNameElement = (
    <Box className="current-theme" sx={{ display: 'none' }}>
      <span className="text">{currentTheme}</span>
    </Box>
  );

  return (
    <>
      {floatingButton}
      {themeMenu}
      {themeNameElement}
    </>
  );
};

// Helper function to determine if text should be white or black based on background
function isContrastingWithBackground(color: string): boolean {
  // Simple contrast check for hex colors
  if (color.startsWith('#')) {
    const r = parseInt(color.slice(1, 3), 16);
    const g = parseInt(color.slice(3, 5), 16);
    const b = parseInt(color.slice(5, 7), 16);
    return r * 0.299 + g * 0.587 + b * 0.114 < 128;
  }
  return true;
}

export default ThemeSelector;
