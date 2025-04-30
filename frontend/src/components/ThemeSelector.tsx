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
  Divider,
} from '@mui/material';
import React, { useRef, useEffect, useState } from 'react';

import * as ThemeController from '../services/themeService';

interface Theme {
  id: string;
  name: string;
  file: string;
  colors: string[];
}

// Main theme selector component with floating button and dropdown
const ThemeSelector: React.FC = () => {
  const [themes, setThemes] = useState<Theme[]>([]);
  const [currentTheme, setCurrentTheme] = useState<string>(
    localStorage.getItem('theme') || 'dark',
  );
  const [open, setOpen] = useState(false);
  const [previewingTheme, setPreviewingTheme] = useState<string | null>(
    null,
  );
  const [loading, setLoading] = useState(false);
  const anchorRef = useRef<HTMLDivElement>(null);
  const previewTimerRef = useRef<number | null>(null);

  useEffect(() => {
    // Load the theme from localStorage on component mount
    const savedTheme = localStorage.getItem('theme') || 'dark';
    setCurrentTheme(savedTheme);

    // Fetch themes from the JSON file
    fetch('/themes/themes.json')
      .then((response) => response.json())
      .then((data) => {
        setThemes(data.themes);
      })
      .catch((error) => {
        console.error('Error loading themes:', error);
      });

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
                            display: 'flex',
                            mr: 2,
                          }}
                        >
                          {/* Theme color swatches */}
                          {theme.colors.map((color, colorIndex) => (
                            <Box
                              key={colorIndex}
                              sx={{
                                width: 20,
                                height: 20,
                                ml: colorIndex > 0 ? -1 : 0,
                                zIndex: 3 - colorIndex,
                                borderRadius: '50%',
                                bgcolor: color,
                                border: '1px solid var(--sub-alt-color)',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                              }}
                            >
                              {currentTheme === theme.id && colorIndex === 0 && (
                                <CheckIcon
                                  sx={{
                                    fontSize: 14,
                                    color: isContrastingWithBackground(color)
                                      ? '#fff'
                                      : '#000',
                                  }}
                                />
                              )}
                            </Box>
                          ))}
                        </Box>
                        <ListItemText
                          primary={
                            <Typography
                              variant="body1"
                              sx={{
                                fontWeight:
                                  currentTheme === theme.id ? 'bold' : 'normal',
                              }}
                            >
                              {theme.name}
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

  return (
    <>
      {floatingButton}
      {themeMenu}
    </>
  );
};

// Helper function to determine if a color should use white or black text
function isContrastingWithBackground(color: string): boolean {
  if (!color) return true;

  if (color.startsWith('#')) {
    let hex = color.slice(1);
    if (hex.length === 3) {
      hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    }
    const r = parseInt(hex.slice(0, 2), 16);
    const g = parseInt(hex.slice(2, 4), 16);
    const b = parseInt(hex.slice(4, 6), 16);
    // Calculate luminance - dark colors need bright text
    return r * 0.299 + g * 0.587 + b * 0.114 < 128;
  }

  return true; // Default to white text on complex colors
}

export default ThemeSelector;
