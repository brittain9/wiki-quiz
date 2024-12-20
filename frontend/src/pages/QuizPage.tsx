import React, { useState } from 'react';
import { PaletteMode } from '@mui/material';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded';
import AppAppBar from '../components/AppBar/AppAppBar';
import Hero from '../components/Hero/Hero';
import Highlights from '../components/Highlights';
import Footer from '../components/Footer';
import getTheme from '../getTheme';
import QuizComponent from '../components/QuizComponent';
import SubmissionHistory from '../components/SubmissionHistory';
import { QuizOptionsProvider } from '../context/QuizOptionsContext';
import { QuizStateProvider } from '../context/QuizStateContext';

interface ToggleCustomThemeProps {
  showCustomTheme: Boolean;
  toggleCustomTheme: () => void;
}

function ToggleCustomTheme({
  showCustomTheme,
  toggleCustomTheme,
}: ToggleCustomThemeProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        width: '100dvw',
        position: 'fixed',
        bottom: 24,
      }}
    >
      <ToggleButtonGroup
        color="primary"
        exclusive
        value={showCustomTheme}
        onChange={toggleCustomTheme}
        aria-label="Toggle design language"
        sx={{
          backgroundColor: 'background.default',
          '& .Mui-selected': {
            pointerEvents: 'none',
          },
        }}
      >
        <ToggleButton value>
          <AutoAwesomeRoundedIcon sx={{ fontSize: '20px', mr: 1 }} />
          Custom theme
        </ToggleButton>
        <ToggleButton data-screenshot="toggle-default-theme" value={false}>
          Material Design 2
        </ToggleButton>
      </ToggleButtonGroup>
    </Box>
  );
}

export default function QuizPage() {
  const [mode, setMode] = React.useState<PaletteMode>('light');
  const [showCustomTheme, setShowCustomTheme] = React.useState(true);
  const QPtheme = createTheme(getTheme(mode));
  const defaultTheme = createTheme({ palette: { mode } });

  const toggleColorMode = () => {
    setMode((prev) => (prev === 'dark' ? 'light' : 'dark'));
  };

  const toggleCustomTheme = () => {
    setShowCustomTheme((prev) => !prev);
  };

  return (
    <QuizOptionsProvider>
      <QuizStateProvider>
        <ThemeProvider theme={showCustomTheme ? QPtheme : defaultTheme}>
          <CssBaseline />
          <AppAppBar 
            mode={mode} 
            toggleColorMode={toggleColorMode}
          />
          <Hero />
          <Box sx={{ bgcolor: 'background.default' }}>
            <QuizComponent />
            <Divider />
            <SubmissionHistory />
            <Divider />
            <Highlights />
            <Divider />
            <Footer />
          </Box>
          <ToggleCustomTheme
            showCustomTheme={showCustomTheme}
            toggleCustomTheme={toggleCustomTheme}
          />
        </ThemeProvider>
      </QuizStateProvider>
    </QuizOptionsProvider>
  );
}