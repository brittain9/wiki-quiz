import React from 'react';
import { PaletteMode } from '@mui/material';
import CssBaseline from '@mui/material/CssBaseline';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import AppAppBar from '../components/AppBar/AppAppBar';
import Hero from '../components/Hero/Hero';
import Highlights from '../components/Highlights';
import Footer from '../components/Footer';
import { getTheme } from '../getTheme';
import QuizComponent from '../components/QuizComponent';
import SubmissionHistory from '../components/SubmissionHistory';
import { QuizOptionsProvider } from '../context/QuizOptionsContext';
import { QuizStateProvider } from '../context/QuizStateContext';

export default function QuizPage() {
  const [mode, setMode] = React.useState<PaletteMode>('light');
  const QPtheme = createTheme(getTheme(mode));

  const toggleColorMode = () => {
    setMode((prev) => (prev === 'dark' ? 'light' : 'dark'));
  };

  return (
    <QuizOptionsProvider>
      <QuizStateProvider>
        <ThemeProvider theme={QPtheme}>
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
        </ThemeProvider>
      </QuizStateProvider>
    </QuizOptionsProvider>
  );
}