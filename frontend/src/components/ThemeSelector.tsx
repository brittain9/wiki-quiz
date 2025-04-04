import { ColorLens, Settings } from '@mui/icons-material';
import {
  Box,
  IconButton,
  Menu,
  MenuItem,
  Tooltip,
  Typography,
} from '@mui/material';
import React, { useState, useEffect, useMemo, useCallback, memo } from 'react';
import { useTranslation } from 'react-i18next';

import { useCustomTheme } from '../context/CustomThemeContext';
import {
  THEME_IDS,
  getTheme,
  getThemePreviewColors,
  ThemeName,
} from '../themes';

// Theme menu item component for better performance
const ThemeMenuItem = memo<{
  theme: ThemeName;
  isSelected: boolean;
  onSelect: () => void;
  onHover: () => void;
  showCurrentLabel: boolean;
}>(({ theme, isSelected, onSelect, onHover, showCurrentLabel }) => {
  const { t } = useTranslation();
  const colors = getThemePreviewColors(theme);

  return (
    <MenuItem
      onClick={onSelect}
      onMouseEnter={onHover}
      selected={isSelected}
      sx={{
        position: 'relative',
        color: 'var(--text-color)',
        '&:hover': {
          bgcolor: 'var(--bg-color-secondary)',
        },
        '&.Mui-selected': {
          bgcolor: 'var(--main-color-10)',
          '&:hover': {
            bgcolor: 'var(--main-color-20)',
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

      {/* Theme name with display name from theme definition */}
      <Typography sx={{ flex: 1 }}>{getTheme(theme).displayName}</Typography>

      {/* "Current" label for selected theme */}
      {showCurrentLabel && (
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
});

ThemeMenuItem.displayName = 'ThemeMenuItem';

/**
 * Theme selector component that allows users to choose a theme
 */
const ThemeSelector = memo(() => {
  const { t } = useTranslation();
  const {
    userSelectedTheme,
    systemTheme,
    setTheme,
    setSystemPreferenceTheme,
    previewTheme,
  } = useCustomTheme();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const isSystemPreferenceActive = userSelectedTheme === null;

  const handleClick = useCallback(
    (event: React.MouseEvent<HTMLButtonElement>) => {
      setAnchorEl(event.currentTarget);
    },
    [],
  );

  const handleClose = useCallback(() => {
    setAnchorEl(null);
  }, []);

  const handleThemeSelect = useCallback(
    (themeName: ThemeName) => {
      setTheme(themeName);
      setAnchorEl(null);
    },
    [setTheme],
  );

  const handleSystemPreferenceSelect = useCallback(() => {
    setSystemPreferenceTheme();
    setAnchorEl(null);
  }, [setSystemPreferenceTheme]);

  const handleThemeHover = useCallback(
    (themeName: ThemeName | null) => {
      previewTheme(themeName);
    },
    [previewTheme],
  );

  const handleSystemPreferenceHover = useCallback(() => {
    previewTheme(systemTheme);
  }, [previewTheme, systemTheme]);

  const handleMenuMouseLeave = useCallback(() => {
    previewTheme(null);
  }, [previewTheme]);

  const systemPreferenceColors = useMemo(() => {
    return getThemePreviewColors(systemTheme);
  }, [systemTheme]);

  return (
    <Box
      className="theme-selector-container"
      sx={{ position: 'fixed', bottom: '2rem', right: '2rem', zIndex: 1000 }}
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
        onMouseLeave={handleMenuMouseLeave}
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

        <MenuItem
          onClick={handleSystemPreferenceSelect}
          onMouseEnter={handleSystemPreferenceHover}
          selected={isSystemPreferenceActive}
          sx={{
            position: 'relative',
            color: 'var(--text-color)',
            '&:hover': {
              bgcolor: 'var(--bg-color-secondary)',
            },
            '&.Mui-selected': {
              bgcolor: 'var(--main-color-10)',
              '&:hover': {
                bgcolor: 'var(--main-color-20)',
              },
            },
          }}
        >
          <Box
            sx={{
              width: 24,
              height: 24,
              borderRadius: '50%',
              mr: 2,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: systemPreferenceColors.bg,
              border: '1px solid',
              borderColor: 'var(--sub-color)',
            }}
          >
            <Settings fontSize="small" />
          </Box>

          <Typography sx={{ flex: 1 }}>
            {t('theme.systemPreference', 'System Preference')}
          </Typography>

          {isSystemPreferenceActive && (
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

        {THEME_IDS.map((theme) => (
          <ThemeMenuItem
            key={theme}
            theme={theme}
            isSelected={userSelectedTheme === theme}
            onSelect={() => handleThemeSelect(theme)}
            onHover={() => handleThemeHover(theme)}
            showCurrentLabel={userSelectedTheme === theme}
          />
        ))}
      </Menu>
    </Box>
  );
});

ThemeSelector.displayName = 'ThemeSelector';

export default ThemeSelector;
