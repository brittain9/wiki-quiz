import { Box, Typography, Tooltip } from '@mui/material';
import React from 'react';

import * as ThemeController from '../services/theme-controller';

interface Theme {
  id: string;
  name: string;
  color: string;
}

const themes: Theme[] = [
  { id: 'dark', name: 'Dark', color: '#7c3aed' },
  { id: 'light', name: 'Light', color: '#6d28d9' },
  // Add more themes as needed
];

const ThemeSelector: React.FC = () => {
  const [currentTheme, setCurrentTheme] = React.useState<string>(
    localStorage.getItem('theme') || 'dark',
  );

  React.useEffect(() => {
    // Load the theme from localStorage on component mount
    const savedTheme = localStorage.getItem('theme') || 'dark';
    setCurrentTheme(savedTheme);
  }, []);

  const handleThemeChange = async (themeId: string) => {
    await ThemeController.setTheme(themeId);
    setCurrentTheme(themeId);
  };

  return (
    <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
      <Typography variant="body2" sx={{ color: 'var(--sub-color)', mr: 1 }}>
        Theme:
      </Typography>

      <Box sx={{ display: 'flex', gap: 1 }}>
        {themes.map((theme) => (
          <Tooltip key={theme.id} title={theme.name} arrow placement="bottom">
            <Box
              onClick={() => handleThemeChange(theme.id)}
              onMouseEnter={() => ThemeController.preview(theme.id)}
              onMouseLeave={() => ThemeController.clearPreview()}
              sx={{
                width: 24,
                height: 24,
                borderRadius: '50%',
                backgroundColor: theme.color,
                cursor: 'pointer',
                transition: 'transform 0.2s, box-shadow 0.2s',
                border: '2px solid',
                borderColor:
                  currentTheme === theme.id
                    ? 'var(--text-color)'
                    : 'transparent',
                '&:hover': {
                  transform: 'scale(1.1)',
                  boxShadow: '0 0 0 2px var(--sub-alt-color)',
                },
              }}
            />
          </Tooltip>
        ))}
      </Box>

      <Box className="current-theme" sx={{ display: 'none' }}>
        <span className="text">{currentTheme}</span>
      </Box>
    </Box>
  );
};

export default ThemeSelector;
