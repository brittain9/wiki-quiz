import { ColorLens } from '@mui/icons-material';
import {
  Box,
  IconButton,
  Menu,
  MenuItem,
  Tooltip,
  Typography,
} from '@mui/material';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';

import { useCustomTheme } from '../context/CustomThemeContext';
import {
  AVAILABLE_THEMES,
  THEME_DISPLAY_NAMES,
  THEME_PREVIEW_COLORS,
  ThemeName,
} from '../themes';

const ThemeSelector: React.FC = () => {
  const { t } = useTranslation();
  const { currentTheme, setTheme } = useCustomTheme();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleThemeSelect = (themeName: ThemeName) => {
    setTheme(themeName);
    handleClose();
  };

  return (
    <Box
      className="theme-selector-container"
      sx={{
        position: 'fixed',
        bottom: '2rem',
        right: '2rem',
        zIndex: 1000,
      }}
    >
      <Tooltip title={t('theme.selector')} arrow>
        <IconButton
          onClick={handleClick}
          size="large"
          aria-controls={open ? 'theme-menu' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
          className="theme-selector-button"
          sx={{
            bgcolor: 'var(--bg-color-secondary)',
            color: 'var(--main-color)',
            border: '1px solid var(--sub-color)',
            '&:hover': {
              bgcolor: 'var(--bg-color-tertiary)',
            },
          }}
        >
          <ColorLens />
        </IconButton>
      </Tooltip>
      <Menu
        id="theme-menu"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        MenuListProps={{
          'aria-labelledby': 'theme-selector-button',
        }}
        PaperProps={{
          sx: {
            bgcolor: 'var(--bg-color)',
            color: 'var(--text-color)',
            border: '1px solid var(--sub-color)',
            minWidth: 200,
            maxHeight: '80vh',
            overflow: 'auto',
            mt: 1.5,
            '& .MuiMenuItem-root': {
              py: 1.5,
            },
          },
        }}
      >
        <Typography
          variant="subtitle1"
          sx={{
            px: 2,
            py: 1,
            fontWeight: 'bold',
            color: 'var(--text-color)',
            borderBottom: '1px solid var(--sub-alt-color)',
          }}
        >
          {t('theme.title')}
        </Typography>

        {AVAILABLE_THEMES.map((theme) => {
          const colors = THEME_PREVIEW_COLORS[theme];
          return (
            <MenuItem
              key={theme}
              onClick={() => handleThemeSelect(theme)}
              selected={currentTheme === theme}
              sx={{
                position: 'relative',
                color: 'var(--text-color)',
                '&:hover': {
                  bgcolor: 'var(--bg-color-secondary)',
                },
                '&.Mui-selected': {
                  bgcolor: 'rgba(var(--main-color-rgb), 0.1)',
                  '&:hover': {
                    bgcolor: 'rgba(var(--main-color-rgb), 0.2)',
                  },
                },
              }}
            >
              {/* Theme color preview swatch */}
              <Box
                sx={{
                  width: 24,
                  height: 24,
                  borderRadius: '50%',
                  mr: 2,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  bgcolor: colors.bg,
                  border: '1px solid',
                  borderColor: 'var(--sub-color)',
                }}
              >
                <Box
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    bgcolor: colors.main,
                  }}
                />
              </Box>

              {/* Theme name with display name from constants */}
              <Typography sx={{ flex: 1 }}>
                {THEME_DISPLAY_NAMES[theme]}
              </Typography>

              {/* "Current" label for selected theme */}
              {currentTheme === theme && (
                <Typography
                  variant="caption"
                  sx={{
                    ml: 1,
                    color: 'var(--main-color)',
                    fontStyle: 'italic',
                  }}
                >
                  {t('theme.current')}
                </Typography>
              )}
            </MenuItem>
          );
        })}
      </Menu>
    </Box>
  );
};

export default ThemeSelector;
